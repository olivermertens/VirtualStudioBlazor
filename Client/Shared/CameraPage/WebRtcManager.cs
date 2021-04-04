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

namespace VirtualStudio.Client.Shared.CameraPage
{
    public class WebRtcManager : IWebRtcClientMethods, IAsyncDisposable
    {
        private readonly IJSRuntime jsRuntime;
        private readonly DotNetObjectReference<WebRtcManager> objRef;
        private readonly HubConnection hubConnection;
        private readonly List<IDisposable> handlers = new List<IDisposable>();
        private readonly StudioComponentDto studioComponent;
        private readonly List<Connection> connections = new List<Connection>();
        private readonly ElementReference receivingVideoElement;
        private readonly ElementReference rtpTimestamp;
        private int handlerId;

        public static async Task<WebRtcManager> CreateAsync(HubConnection hubConnection, StudioComponentDto studioComponent, IJSRuntime jsRuntime, ElementReference receivingVideoElement, ElementReference rtpTimestamp)
        {
            var manager = new WebRtcManager(hubConnection, studioComponent, jsRuntime, receivingVideoElement, rtpTimestamp);
            manager.handlerId = await jsRuntime.InvokeAsync<int>("WebRtcHandlerManager.createHandler");
            return manager;
        }

        private WebRtcManager(HubConnection hubConnection, StudioComponentDto studioComponent, IJSRuntime jsRuntime, ElementReference receivingVideoElement, ElementReference rtpTimestamp)
        {
            this.studioComponent = studioComponent;
            this.hubConnection = hubConnection;
            this.jsRuntime = jsRuntime;
            this.receivingVideoElement = receivingVideoElement;
            this.rtpTimestamp = rtpTimestamp;
            objRef = DotNetObjectReference.Create(this);
            handlers.Add(hubConnection.On<int, int, DataKind>(nameof(RequestSdpOffer), RequestSdpOffer));
            handlers.Add(hubConnection.On<int, int, DataKind, string, bool>(nameof(RequestSdpAnswer), RequestSdpAnswer));
            handlers.Add(hubConnection.On<int, RtcIceCandidateInit>(nameof(AddIceCandidate), AddIceCandidate));
            handlers.Add(hubConnection.On<int, string, bool>(nameof(Connect), Connect));
            handlers.Add(hubConnection.On<int>(nameof(Disconnect), Disconnect));
        }

        public async ValueTask DisposeAsync()
        {
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.disposeHandler", handlerId);
            foreach (var handler in handlers)
                handler.Dispose();
        }

        public async Task OpenCameraAsync(ElementReference? sendingVideoElement)
        {
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.openCameraStream", handlerId, sendingVideoElement);
        }


        #region HubConnectionEventHandlers
        public async Task RequestSdpOffer(int connectionId, int endPointId, DataKind dataKind)
        {
            Console.WriteLine("RequestSdpOffer");
            var output = GetOutput(endPointId);
            if (output is null)
                throw new InvalidOperationException($"Output with ID {endPointId} and ConnectionType 'WebRTC' does not exist.");

            var connectionDataKind = dataKind & output.DataKind;
            if (connectionDataKind == DataKind.Nothing)
                throw new InvalidOperationException($"DataKinds to not match.");

            bool supportsInsertableStreams = await jsRuntime.InvokeAsync<bool>("WebRtcHandlerManager.areInsertableStreamsSupported", handlerId);
            var sdpOffer = await jsRuntime.InvokeAsync<string>("WebRtcHandlerManager.getSdpOffer", handlerId, connectionId, (int)connectionDataKind, objRef);

            var connection = new Connection { Id = connectionId, Endpoint = output, DataKind = connectionDataKind, State = ConnectionState.Disconnected };
            connections.Add(connection);

            await hubConnection.SendAsync("RespondSdpOffer", connectionId, sdpOffer, supportsInsertableStreams);
        }

        private StudioComponentEndpointDto GetInput(int inputId)
        {
            return studioComponent.Inputs.FirstOrDefault(i => i.Id == inputId && i.ConnectionType == "WebRTC");
        }

        private StudioComponentEndpointDto GetOutput(int outputId)
        {
            return studioComponent.Outputs.FirstOrDefault(o => o.Id == outputId && o.ConnectionType == "WebRTC");
        }

        public async Task RequestSdpAnswer(int connectionId, int endPointId, DataKind dataKind, string sdpOffer, bool remotePeerSupportsInsertableStreams)
        {
            Console.WriteLine("RequestSdpAnswer");
            var supportsInsertableStreams = await jsRuntime.InvokeAsync<bool>("WebRtcHandlerManager.areInsertableStreamsSupported", handlerId);
            var sdpAnswer = await jsRuntime.InvokeAsync<string>("WebRtcHandlerManager.getSdpAnswer", handlerId, connectionId, sdpOffer, remotePeerSupportsInsertableStreams, receivingVideoElement, rtpTimestamp, objRef);
            await hubConnection.SendAsync("RespondSdpAnswer", connectionId, sdpAnswer, remotePeerSupportsInsertableStreams && supportsInsertableStreams);
        }

        public async Task AddIceCandidate(int connectionId, RtcIceCandidateInit candidateJson)
        {
            Console.WriteLine("AddIceCandidate: " + candidateJson.Candidate);
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.addIceCandidate",
                handlerId,
                connectionId,
                candidateJson.Candidate,
                candidateJson.SdpMid,
                candidateJson.SdpMLineIndex,
                candidateJson.UsernameFragment);
        }

        public async Task Connect(int connectionId, string sdpAnswer, bool useInsertableStreams)
        {
            Console.WriteLine("Connect");
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.connect", handlerId, connectionId, sdpAnswer, useInsertableStreams);
        }

        public async Task Disconnect(int connectionId)
        {
            Console.WriteLine("Disconnect");
            await jsRuntime.InvokeVoidAsync("WebRtcHandlerManager.disconnect", handlerId, connectionId);
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
            await hubConnection.SendAsync("SendIceCandidate", connectionId, new RtcIceCandidateInit(candidate, sdpMid, sdpMLineIndex, usernameFragement));
        }

        [JSInvokable]
        public async void OnConnectionStateChanged(int connectionId, bool isInput, string connectionState)
        {
            if (isInput)
                await hubConnection.SendAsync("UpdateInputConnectionState", 1, connectionId, ParseConnectionState(connectionState));
            else
                await hubConnection.SendAsync("UpdateOutputConnectionState", 2, connectionId, ParseConnectionState(connectionState));
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
