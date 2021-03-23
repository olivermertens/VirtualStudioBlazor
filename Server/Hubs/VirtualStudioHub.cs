using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VirtualStudio.ConnectionTypes.WebRtc;
using VirtualStudio.Core;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.StudioClient;

namespace VirtualStudio.Server
{
    public class VirtualStudioHub : Hub<IWebRtcClientMethods>, IStudioClientUpdater, IWebRtcHubMethods
    {
        static ConcurrentDictionary<string, string> connectionIdToGroupMapping = new ConcurrentDictionary<string, string>();
        static ConcurrentDictionary<string, SignalRStudioClient> connectionIdToStudioClientMapping = new ConcurrentDictionary<string, SignalRStudioClient>();

        private readonly VirtualStudioRepository virtualStudios;
        private readonly ILogger<VirtualStudioHub> logger;
        private readonly VirtualStudioClientProvider virtualStudioClientProvider;

        public VirtualStudioHub(VirtualStudioRepository virtualStudios, VirtualStudioClientProvider virtualStudioClientProvider, ILogger<VirtualStudioHub> logger)
        {
            this.virtualStudioClientProvider = virtualStudioClientProvider;
            this.virtualStudios = virtualStudios;
            this.logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            logger.LogInformation($"Client {Context.ConnectionId} connected.");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            logger.LogInformation($"Client {Context.ConnectionId} disconnected.");
            await RemoveClientFromAnyVirtualStudioAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> JoinVirtualStudio(string virtualStudioName, StudioComponentDto studioComponent)
        {
            string connectionId = Context.ConnectionId;
            await RemoveClientFromAnyVirtualStudioAsync(connectionId);
            await AddClientToVirtualStudioAsync(connectionId, virtualStudioName, studioComponent);
            return true;
        }

        public async Task<bool> LeaveVirtualStudio(string virtualStudioName)
        {
            await RemoveClientFromStudioAsync(Context.ConnectionId, virtualStudioName);
            return true;
        }

        #region IStudioClientUpdater
        public Task UpdateInputConnectionState(int inputId, int connectionId, ConnectionState state)
            => CallOnCurrentClient(c => c.UpdateInputConnectionState(inputId, connectionId, state));

        public Task UpdateOutputConnectionState(int outputId, int connectionId, ConnectionState state)
            => CallOnCurrentClient(c => c.UpdateOutputConnectionState(outputId, connectionId, state));

        public Task AddInput(StudioComponentEndpointDto input)
            => CallOnCurrentClient(c => c.AddInput(input));

        public Task RemoveInput(int inputId)
            => CallOnCurrentClient(c => c.RemoveInput(inputId));

        public Task AddOutput(StudioComponentEndpointDto output)
            => CallOnCurrentClient(c => c.AddOutput(output));

        public Task RemoveOutput(int outputId)
            => CallOnCurrentClient(c => c.RemoveOutput(outputId));

        public Task ChangeProperty(string propertyName, object value)
            => CallOnCurrentClient(c => c.ChangeProperty(propertyName, value));

        #endregion

        #region IWebRtcHubMethods
        public Task RespondSdpOffer(int connectionId, string sdpOffer)
        {
            return CallOnCurrentClient(c => c.OnSdpOfferReceived(connectionId, sdpOffer));
        }

        public Task RespondSdpAnswer(int connectionId, string sdpAnswer)
        {
            return CallOnCurrentClient(c => c.OnSdpAnswerReceived(connectionId, sdpAnswer));
        }

        public Task SendIceCandidate(int connectionId, RtcIceCandidateInit candidateJson)
        {
            return CallOnCurrentClient(c => c.OnIceCandidateReceived(connectionId, candidateJson));
        }

        #endregion

        private Task CallOnCurrentClient(Action<SignalRStudioClient> action)
        {
            action(connectionIdToStudioClientMapping[Context.ConnectionId]);
            return Task.CompletedTask;
        }

        private async Task AddClientToVirtualStudioAsync(string connectionId, string virtualStudioName, StudioComponentDto studioComponentDto)
        {
            await Groups.AddToGroupAsync(connectionId, virtualStudioName);
            connectionIdToGroupMapping[connectionId] = virtualStudioName;
            logger.LogInformation($"StudioClient '{Context.ConnectionId}' joined {virtualStudioName}");
            if (virtualStudios.TryGetVirtualStudio(virtualStudioName, out Core.VirtualStudio virtualStudio))
            {
                var studioClient = new SignalRStudioClient(virtualStudioName, connectionId, virtualStudioClientProvider, studioComponentDto);
                connectionIdToStudioClientMapping[connectionId] = studioClient;
                virtualStudio.ComponentRepository.AddClient(studioClient.StudioComponent);
                virtualStudio.AddComponent(studioClient.StudioComponent);
            }
        }

        private async Task RemoveClientFromAnyVirtualStudioAsync(string connectionId)
        {
            if (connectionIdToGroupMapping.TryGetValue(connectionId, out string groupName) && groupName != null)
                await RemoveClientFromStudioAsync(connectionId, groupName);
        }

        private async Task RemoveClientFromStudioAsync(string connectionId, string virtualStudioName)
        {
            if (connectionIdToGroupMapping.TryGetValue(connectionId, out string groupName) && groupName == virtualStudioName)
            {
                if (virtualStudios.TryGetVirtualStudio(virtualStudioName, out Core.VirtualStudio virtualStudio))
                {
                    var studioClient = connectionIdToStudioClientMapping[connectionId];
                    virtualStudio.RemoveComponent(studioClient.StudioComponent);
                    virtualStudio.ComponentRepository.RemoveClient(studioClient.StudioComponent);
                }
                await Groups.RemoveFromGroupAsync(connectionId, virtualStudioName);
                connectionIdToGroupMapping[connectionId] = null;
                logger.LogInformation($"StudioClient '{Context.ConnectionId}' left {virtualStudioName}");
            }
        }
    }
}