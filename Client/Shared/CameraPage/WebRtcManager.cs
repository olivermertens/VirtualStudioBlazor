using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;

namespace VirtualStudio.Client.Shared.CameraPage
{
    public class WebRtcManager : IWebRtcClient, IAsyncDisposable
    {
        private readonly IJSRuntime jsRuntime;
        private readonly DotNetObjectReference<WebRtcManager> objRef;
        private readonly IVirtualStudioConnection virtualStudioConnection;
        private readonly List<IDisposable> handlers = new List<IDisposable>();
        private readonly StudioComponentDto studioComponent;
        private readonly List<Connection> connections = new List<Connection>();
        private readonly ElementReference receivingVideoElement;
        private readonly ElementReference rtpTimestamp;
        private int handlerId;

        public static async Task<WebRtcManager> CreateAsync(IVirtualStudioConnection virtualStudioConnection, StudioComponentDto studioComponent, IJSRuntime jsRuntime, ElementReference receivingVideoElement, ElementReference rtpTimestamp)
        {
            var manager = new WebRtcManager(virtualStudioConnection, studioComponent, jsRuntime, receivingVideoElement, rtpTimestamp);
            manager.handlerId = await jsRuntime.InvokeAsync<int>("WebRtcHandlerManager.createHandler");
            return manager;
        }

        private WebRtcManager(IVirtualStudioConnection virtualStudioConnection, StudioComponentDto studioComponent, IJSRuntime jsRuntime, ElementReference receivingVideoElement, ElementReference rtpTimestamp)
        {
            this.studioComponent = studioComponent;
            this.virtualStudioConnection = virtualStudioConnection;
            this.jsRuntime = jsRuntime;
            this.receivingVideoElement = receivingVideoElement;
            this.rtpTimestamp = rtpTimestamp;
            objRef = DotNetObjectReference.Create(this);
            virtualStudioConnection.AddListener(this);
        }

        public async ValueTask DisposeAsync()
        {
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.disposeHandler", handlerId);
            virtualStudioConnection.RemoveListener(this);
            foreach (var handler in handlers)
                handler.Dispose();
            objRef.Dispose();
        }

        public async Task OpenCameraAsync(ElementReference? sendingVideoElement)
        {
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.openCameraStream", handlerId, sendingVideoElement);
        }


        #region HubConnectionEventHandlers
        public async Task RequestSdpOffer(SdpOfferRequestArgs args)
        {
            Console.WriteLine("RequestSdpOffer");
            var output = GetOutput(args.EndPointId);
            if (output is null)
                throw new InvalidOperationException($"Output with ID {args.EndPointId} and ConnectionType 'WebRTC' does not exist.");

            var connectionDataKind = args.DataKind & output.DataKind;
            if (connectionDataKind == DataKind.Nothing)
                throw new InvalidOperationException($"DataKinds to not match.");

            bool supportsInsertableStreams = await jsRuntime.InvokeAsync<bool>("WebRtcHandlerManager.areInsertableStreamsSupported", handlerId);
            var sdpOffer = await jsRuntime.InvokeAsync<string>("WebRtcHandlerManager.getSdpOffer", handlerId, args.ConnectionId, (int)connectionDataKind, objRef);

            var connection = new Connection { Id = args.ConnectionId, Endpoint = output, DataKind = connectionDataKind, State = ConnectionState.Disconnected };
            connections.Add(connection);

            await virtualStudioConnection.SendSdpOffer(new SdpOfferResponseArgs
            {
                ConnectionId = args.ConnectionId,
                SdpOffer = sdpOffer,
                SupportsInsertableStreams = supportsInsertableStreams
            });
        }

        private StudioComponentEndpointDto GetInput(int inputId)
        {
            return studioComponent.Inputs.FirstOrDefault(i => i.Id == inputId && i.ConnectionType == "WebRTC");
        }

        private StudioComponentEndpointDto GetOutput(int outputId)
        {
            return studioComponent.Outputs.FirstOrDefault(o => o.Id == outputId && o.ConnectionType == "WebRTC");
        }

        public async Task RequestSdpAnswer(SdpAnswerRequestArgs args)
        {
            Console.WriteLine("RequestSdpAnswer");
            var supportsInsertableStreams = await jsRuntime.InvokeAsync<bool>("WebRtcHandlerManager.areInsertableStreamsSupported", handlerId);
            var sdpAnswer = await jsRuntime.InvokeAsync<string>("WebRtcHandlerManager.getSdpAnswer", handlerId, args.ConnectionId, args.SdpOffer, args.RemotePeerSupportsInsertableStreams, receivingVideoElement, rtpTimestamp, objRef);
            await virtualStudioConnection.SendSdpAnswer(new SdpAnswerResponseArgs
            {
                ConnectionId = args.ConnectionId,
                SdpAnswer = sdpAnswer,
                UseInsertableStreams = args.RemotePeerSupportsInsertableStreams && supportsInsertableStreams
            });
        }

        public async Task AddIceCandidate(IceCandidateArgs args)
        {
            Console.WriteLine("AddIceCandidate: " + args.CandidateDto.Candidate);
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.addIceCandidate",
                handlerId,
                args.ConnectionId,
                args.CandidateDto.Candidate,
                args.CandidateDto.SdpMid,
                args.CandidateDto.SdpMLineIndex,
                args.CandidateDto.UsernameFragment);
        }

        public async Task Connect(ConnectWebRtcCommandArgs args)
        {
            Console.WriteLine("Connect");
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.connect", handlerId, args.ConnectionId, args.SdpAnswer, args.UseInsertableStreams);
        }

        public async Task Disconnect(DisconnectWebRtcCommandArgs args)
        {
            Console.WriteLine("Disconnect");
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.disconnect", handlerId, args.ConnectionId);
        }

        private ConnectionState ParseConnectionState(string connectionState)
        {
            return connectionState switch
            {
                "connecting" => ConnectionState.Connecting,
                "connected" => ConnectionState.Connected,
                "disconnected" => ConnectionState.Disconnected,
                "failed" => ConnectionState.Disconnected,
                _ => ConnectionState.Disconnected
            };
        }

        #endregion

        #region JsEvents
        [JSInvokable]
        public async void OnIceCandidate(int connectionId, string candidate, string sdpMid, int sdpMLineIndex, string usernameFragement)
        {
            Console.WriteLine("OnIceCandidate: " + candidate);
            await virtualStudioConnection.SendIceCandidate(new IceCandidateArgs
            {
                ConnectionId = connectionId,
                CandidateDto = new RtcIceCandidateDto(candidate, sdpMid, sdpMLineIndex, usernameFragement)
            });
        }

        [JSInvokable]
        public async void OnConnectionStateChanged(int connectionId, bool isInput, string connectionState)
        { 
            if (isInput)
                await virtualStudioConnection.UpdateInputConnectionState(1, connectionId, ParseConnectionState(connectionState));
            else
                await virtualStudioConnection.UpdateOutputConnectionState(2, connectionId, ParseConnectionState(connectionState));
        }
        #endregion

        private class Connection
        {
            public int Id { get; set; }
            public StudioComponentEndpointDto Endpoint { get; set; }
            public DataKind DataKind { get; set; }
            public ConnectionState State { get; set; }
        }
    }
}
