using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class VirtualStudioHub : Hub
    {
        private readonly ILogger<VirtualStudioHub> logger;

        public VirtualStudioHub(ILogger<VirtualStudioHub> logger)
        {
            this.logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            logger.LogInformation($"Client {Context.ConnectionId} connected.");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            logger.LogInformation($"Client {Context.ConnectionId} disconnected.");
            return base.OnDisconnectedAsync(exception);
        }
    }
}