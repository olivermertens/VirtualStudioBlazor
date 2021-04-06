using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.ConnectionTypes.WebRtc;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;
using VirtualStudio.StudioClient;

namespace VirtualStudio.Server
{
    public class SignalRStudioClient : IStudioClient
    {
        public event EventHandler<(int inputId, int connectionId, ConnectionState state)> InputConnectionStateUpdated;
        public event EventHandler<(int outputId, int connectionId, ConnectionState state)> OutputConnectionStateUpdated;
        public event EventHandler<StudioComponentEndpointDto> InputAdded;
        public event EventHandler<int> InputRemoved;
        public event EventHandler<StudioComponentEndpointDto> OutputAdded;
        public event EventHandler<int> OutputRemoved;
        public event EventHandler<(string propertyName, object value)> PropertyChanged;
        public event EventHandler<IceCandidateArgs> IceCandidateReceived;

        public StudioClientComponent StudioComponent { get; }
        public string VirtualStudioName { get; }

        private readonly string connectionId;
        private readonly VirtualStudioClientProvider signalRClientProvider;
        private List<WebRtcConnectProcess> connectProcesses = new List<WebRtcConnectProcess>();

        private IWebRtcClient ClientConnection => signalRClientProvider.GetSignalRClient(connectionId);

        public SignalRStudioClient(string virtualStudioName, string connectionId, VirtualStudioClientProvider signalRClientProvider, StudioComponentDto studioComponentDto)
        {
            this.connectionId = connectionId;
            this.signalRClientProvider = signalRClientProvider;
            VirtualStudioName = virtualStudioName ?? throw new ArgumentNullException(nameof(virtualStudioName));
            if (string.IsNullOrWhiteSpace(virtualStudioName))
                throw new ArgumentException(nameof(virtualStudioName));

            StudioComponent = new StudioClientComponent(this, studioComponentDto);
        }

        #region WebRtc
        async Task<(string, bool)> IWebRtcConnectionHandler.GetSdpOffer(IStudioConnection connection)
        {
            var connectProcess = new WebRtcConnectProcess(connection);
            connectProcesses.Add(connectProcess);
            await ClientConnection.RequestSdpOffer(new SdpOfferRequestArgs
            {
                ConnectionId = connection.Id,
                EndPointId = connection.Output.Id,
                DataKind = connection.Input.DataKind
            });
            if (connectProcess.SdpOfferAvailableEvent.WaitOne(5000))
                return (connectProcess.SdpOffer, connectProcess.SenderSupportsInsertableStreams);
            else
                return (null, false);
        }
        async Task<(string, bool)> IWebRtcConnectionHandler.GetSdpAnswer(IStudioConnection connection, string sdpOffer, bool remotePeerSupportsInsertableStreams)
        {
            var connectProcess = new WebRtcConnectProcess(connection);
            connectProcesses.Add(connectProcess);
            await ClientConnection.RequestSdpAnswer(new SdpAnswerRequestArgs
            {
                ConnectionId = connection.Id,
                EndPointId = connection.Input.Id,
                DataKind = connection.Output.DataKind,
                SdpOffer = sdpOffer,
                RemotePeerSupportsInsertableStreams = remotePeerSupportsInsertableStreams
            });
            if (connectProcess.SdpAnswerAvailableEvent.WaitOne(5000))
                return (connectProcess.SdpAnswer, connectProcess.UseInsertableStreams);
            else
                return (null, false);
        }

        Task IWebRtcConnectionHandler.AddIceCandidate(IStudioConnection connection, RtcIceCandidateDto candidateDto)
            => ClientConnection.AddIceCandidate(new IceCandidateArgs { ConnectionId = connection.Id, CandidateDto = candidateDto });
        Task IWebRtcConnectionHandler.Connect(IStudioConnection connection, string spdAnswer, bool useInsertableStreams)
            => ClientConnection.Connect(new ConnectWebRtcCommandArgs { ConnectionId = connection.Id, SdpAnswer = spdAnswer, UseInsertableStreams = useInsertableStreams });
        Task IWebRtcConnectionHandler.Disconnect(IStudioConnection connection)
            => ClientConnection.Disconnect(new DisconnectWebRtcCommandArgs { ConnectionId = connection.Id });
        #endregion

        public void OnSdpOfferReceived(SdpOfferResponseArgs args)
        {
            var connectionProcess = connectProcesses.First(c => c.Connection.Id == args.ConnectionId);
            connectionProcess.SenderSupportsInsertableStreams = args.SupportsInsertableStreams;
            connectionProcess.SetSdpOffer(args.SdpOffer);
        }

        public void OnSdpAnswerReceived(SdpAnswerResponseArgs args)
        {
            var connectionProcess = connectProcesses.First(c => c.Connection.Id == args.ConnectionId);
            connectionProcess.UseInsertableStreams = args.UseInsertableStreams;
            connectionProcess.SetSdpAnswer(args.SdpAnswer);
        }

        public void OnIceCandidateReceived(IceCandidateArgs args)
        {
            IceCandidateReceived?.Invoke(this, args);
        }

        #region EventInvocation
        public void UpdateInputConnectionState(int inputId, int connectionId, ConnectionState state)
            => InputConnectionStateUpdated?.Invoke(this, (inputId, connectionId, state));

        public void UpdateOutputConnectionState(int outputId, int connectionId, ConnectionState state)
            => OutputConnectionStateUpdated?.Invoke(this, (outputId, connectionId, state));

        public void AddInput(StudioComponentEndpointDto input)
            => InputAdded?.Invoke(this, input);

        public void RemoveInput(int inputId)
            => InputRemoved?.Invoke(this, inputId);

        public void AddOutput(StudioComponentEndpointDto output)
            => OutputAdded(this, output);

        public void RemoveOutput(int outputId)
            => OutputRemoved?.Invoke(this, outputId);

        public void ChangeProperty(string propertyName, object value)
            => PropertyChanged?.Invoke(this, (propertyName, value));

        #endregion
    }
}
