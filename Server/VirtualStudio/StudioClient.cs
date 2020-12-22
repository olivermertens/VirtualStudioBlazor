using System;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class StudioClient : StudioClientBase, IVirtualStudioClient
    {
        public event EventHandler ConnectionStateChanged;
        public event EventHandler<LinkStateUpdate> LinkStateChanged;

        public string ConnectionId { get; set; }

        public ClientConnectionState ConnectionState
        {
            get => connectionState;
            set
            {
                if (value != connectionState)
                {
                    connectionState = value;
                    ConnectionStateChanged?.Invoke(this, null);
                }
            }
        }

        public VirtualStudioHub Hub { get; set; }

        private ClientConnectionState connectionState;

        public StudioClient() { }
        public StudioClient(StudioClientBase baseClient)
        {
            Identifier = baseClient.Identifier;
            NetworkInfo = baseClient.NetworkInfo;
            IODescription = baseClient.IODescription;
            TransmissionMethods = baseClient.TransmissionMethods;
        }

        public void UpdateLinkState(LinkStateUpdate update)
        {
            LinkStateChanged?.Invoke(this, update);
        }

        public void ReceiveMessage(string message)
            => GetHubClient()?.ReceiveMessage(message);
        

        public bool InitWebRtcConnection(int linkId, EndpointDescription sendingEndpoint)   
            => GetHubClient()?.InitWebRtcConnection(linkId, sendingEndpoint) ?? false;
        

        public string GetSdpAnswer(string sdpOffer, EndpointDescription receivingEndpoint)       
            => GetHubClient()?.GetSdpAnswer(sdpOffer, receivingEndpoint);
        

        public Task DisconnectLink(int linkId)
            => GetHubClient()?.DisconnectLink(linkId);
        

        private IVirtualStudioClient GetHubClient()
            => Hub?.Clients.Client(ConnectionId);
    }
}