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
        public event EventHandler<(int, RtcIceCandidateInit)> IceCandidateReceived;

        public StudioClientComponent StudioComponent { get; }
        public string VirtualStudioName { get; }

        private readonly string connectionId;
        private readonly VirtualStudioClientProvider signalRClientProvider;
        private List<WebRtcConnectProcess> connectProcesses = new List<WebRtcConnectProcess>();

        private IWebRtcClientMethods ClientConnection => signalRClientProvider.GetSignalRClient(connectionId);

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
        async Task<string> IWebRtcConnectionHandler.GetSdpOffer(IStudioConnection connection)
        {
            var connectProcess = new WebRtcConnectProcess(connection);
            connectProcesses.Add(connectProcess);
            await ClientConnection.RequestSdpOffer(connection.Id, connection.Output.Id, connection.Input.DataKind);
            if (connectProcess.SdpOfferAvailableEvent.WaitOne(5000))
                return connectProcess.SdpOffer;
            else
                return null;
        }
        async Task<string> IWebRtcConnectionHandler.GetSdpAnswer(IStudioConnection connection, string sdpOffer)
        {
            var connectProcess = new WebRtcConnectProcess(connection);
            connectProcesses.Add(connectProcess);
            await ClientConnection.RequestSdpAnswer(connection.Id, connection.Input.Id, connection.Output.DataKind, sdpOffer);
            if (connectProcess.SdpAnswerAvailableEvent.WaitOne(5000))
                return connectProcess.SdpAnswer;
            else
                return null;
        }

        Task IWebRtcConnectionHandler.AddIceCandidate(IStudioConnection connection, RtcIceCandidateInit candidateJson) => ClientConnection.AddIceCandidate(connection.Id, candidateJson);
        Task IWebRtcConnectionHandler.Connect(IStudioConnection connection, string spdAnswer) => ClientConnection.Connect(connection.Id, spdAnswer);
        Task IWebRtcConnectionHandler.Disconnect(IStudioConnection connection) => ClientConnection.Disconnect(connection.Id);
        #endregion

        public void OnSdpOfferReceived(int connectionId, string sdpOffer)
        {
            var connectionProcess = connectProcesses.First(c => c.Connection.Id == connectionId);
            connectionProcess.SetSdpOffer(sdpOffer);
        }

        public void OnSdpAnswerReceived(int connectionId, string sdpAnswer)
        {
            var connectionProcess = connectProcesses.First(c => c.Connection.Id == connectionId);
            connectionProcess.SetSdpAnswer(sdpAnswer);
        }

        public void OnIceCandidateReceived(int connectionId, RtcIceCandidateInit candidateJson)
        {
            IceCandidateReceived?.Invoke(this, (connectionId, candidateJson));
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
