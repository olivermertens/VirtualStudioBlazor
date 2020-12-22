using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class VirtualStudioHub : Hub<IVirtualStudioClient>
    {
        private readonly StudioManager studioManager;

        public VirtualStudioHub(StudioManager studioManager)
        {
            this.studioManager = studioManager;
            this.studioManager.RegisterHub(this);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client {Context.ConnectionId} connected.");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            studioManager.GetClient(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public bool RegisterStudioClient(StudioClientBase studioClient)
        {
            var newClient = new StudioClient(studioClient);
            newClient.ConnectionId = Context.ConnectionId;
            newClient.Hub = this;
            studioManager.AddStudioClient(newClient);
            Console.WriteLine($"Registered studio client {newClient.Identifier}.");
            return true;
        }

        public void UpdateLinkState(LinkStateUpdate data)
        {
            var client = studioManager.GetClient(Context.ConnectionId);
            if(client == null)
            {
                throw new NotImplementedException();
            }
            client.UpdateLinkState(data);
        }
    }
}