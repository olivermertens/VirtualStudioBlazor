using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.ViewModels;

namespace VirtualStudio.Client
{
    public class SignalRVirtualStudioViewModelUpdater : VirtualStudioViewModelUpdater, IDisposable
    {
        private List<IDisposable> attachedHandlers;

        public SignalRVirtualStudioViewModelUpdater(VirtualStudioViewModel virtualStudioViewModel, HubConnection hubConnection) : base(virtualStudioViewModel)
        {
            AttachToHubConnection(hubConnection);
        }

        public void DetachFromHubConnection()
        {
            if (attachedHandlers is null)
                return;

            foreach (var handler in attachedHandlers)
                handler.Dispose();

            attachedHandlers = null;
        }

        public void Dispose()
        {
            DetachFromHubConnection();
        }

        private IList<IDisposable> AttachToHubConnection(HubConnection hubConnection)
        {
            var attachedHandlers = new List<IDisposable>();
            attachedHandlers.Add(hubConnection.On<string, StudioComponentDto>(nameof(AddClientToRepository), (virtualStudioName, dto) => AddClientToRepository(virtualStudioName, dto)));
            attachedHandlers.Add(hubConnection.On<string, StudioComponentDto>(nameof(AddPlaceholderToRepository), (virtualStudioName, dto) => AddPlaceholderToRepository(virtualStudioName, dto)));
            attachedHandlers.Add(hubConnection.On<string, int, float, float>(nameof(AddComponentNode), (virtualStudioName, id, x, y) => AddComponentNode(virtualStudioName, id, x, y)));
            attachedHandlers.Add(hubConnection.On<string, StudioComponentDto, float, float>(nameof(AddPlaceholderNode), (virtualStudioName, dto, x, y) => AddPlaceholderNode(virtualStudioName, dto, x, y)));
            attachedHandlers.Add(hubConnection.On<string, int, StudioComponentEndpointDto>(nameof(AddInputToComponent), (virtualStudioName, id, dto) => AddInputToComponent(virtualStudioName, id, dto)));
            attachedHandlers.Add(hubConnection.On<string, int, StudioComponentEndpointDto>(nameof(AddOutputToComponent), (virtualStudioName, id, dto) => AddOutputToComponent(virtualStudioName, id, dto)));
            attachedHandlers.Add(hubConnection.On<string, int, string, object>(nameof(ChangeComponentProperty), (virtualStudioName, id, propName, value) => ChangeComponentProperty(virtualStudioName, id, propName, value)));
            attachedHandlers.Add(hubConnection.On<string, int, ConnectionState>(nameof(ChangeConnectionState), (virtualStudioName, id, state) => ChangeConnectionState(virtualStudioName, id, state)));
            attachedHandlers.Add(hubConnection.On<string, StudioConnectionDto>(nameof(CreateConnection), (virtualStudioName, dto) => CreateConnection(virtualStudioName, dto)));
            attachedHandlers.Add(hubConnection.On<string, int, float, float>(nameof(MoveComponentNode), (virtualStudioName, id, x, y) => MoveComponentNode(virtualStudioName, id, x, y)));
            attachedHandlers.Add(hubConnection.On<string, int>(nameof(RemoveClientFromRepository), (virtualStudioName, id) => RemoveClientFromRepository(virtualStudioName, id)));
            attachedHandlers.Add(hubConnection.On<string, int>(nameof(RemovePlaceholderFromRepository), (virtualStudioName, id) => RemovePlaceholderFromRepository(virtualStudioName, id)));
            attachedHandlers.Add(hubConnection.On<string, int>(nameof(RemoveComponent), (virtualStudioName, id) => RemoveComponent(virtualStudioName, id)));
            attachedHandlers.Add(hubConnection.On<string, int>(nameof(RemoveConnection), (virtualStudioName, id) => RemoveConnection(virtualStudioName, id)));
            attachedHandlers.Add(hubConnection.On<string, int, int>(nameof(RemoveInputFromComponent), (virtualStudioName, compId, id) => RemoveInputFromComponent(virtualStudioName, compId, id)));
            attachedHandlers.Add(hubConnection.On<string, int, int>(nameof(RemoveOutputFromComponent), (virtualStudioName, compId, id) => RemoveOutputFromComponent(virtualStudioName, compId, id)));
            return attachedHandlers;
        }
    }
}
