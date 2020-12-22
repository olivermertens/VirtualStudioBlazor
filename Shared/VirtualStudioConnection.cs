using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace VirtualStudio.Shared
{
    public class VirtualStudioConnection : IVirtualStudioClient
    {
        const string VIRTUAL_STUDIO_HUB_ROUTE = "VirtualStudioHub";

        string host;
        int port;
        HubConnection hubConnection;

        public VirtualStudioConnection(string host, int port)
            => (this.host, this.port) = (host, port);

        public async Task Connect()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl($"ws://{host}:{port}/{VIRTUAL_STUDIO_HUB_ROUTE}")
                .Build();
                
            await hubConnection.StartAsync();
        }

        public Task DisconnectLink(int linkId)
        {
            throw new System.NotImplementedException();
        }

        public string GetSdpAnswer(string sdpOffer, EndpointDescription receivingEndpoint)
        {
            throw new System.NotImplementedException();
        }

        public bool InitWebRtcConnection(int linkId, EndpointDescription sendingEndpoint)
        {
            throw new System.NotImplementedException();
        }

        public void ReceiveMessage(string message)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RegisterStudioClient(StudioClientBase studioClient)
            => hubConnection.InvokeAsync<bool>(nameof(RegisterStudioClient), studioClient);

    }
}