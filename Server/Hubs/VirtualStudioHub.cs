using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class VirtualStudioHub : Hub<IVirtualStudioClient>
    {
        public VirtualStudioHub()
        {
            
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client {Context.ConnectionId} connected.");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client {Context.ConnectionId} disconnected.");
            return base.OnDisconnectedAsync(exception);
        }

        public bool RegisterStudioClient(StudioClientBase studioClient)
        {
            return false;
        }
    }
}