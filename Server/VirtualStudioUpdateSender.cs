using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Server
{
    public class VirtualStudioUpdateSender : IVirtualStudioUpdateListener
    {
        private readonly IHubContext<ControlHub, IVirtualStudioUpdateListener> controlHubContext;

        public VirtualStudioUpdateSender(IHubContext<ControlHub, IVirtualStudioUpdateListener> controlHubContext)
        {
            this.controlHubContext = controlHubContext ?? throw new ArgumentNullException(nameof(controlHubContext));
        }

        public Task AddClientToRepository(string virtualStudioName, StudioComponentDto component) => GetGroup(virtualStudioName).AddClientToRepository(virtualStudioName, component);
        public Task AddComponent(string virtualStudioName, int componentId) => GetGroup(virtualStudioName).AddComponent(virtualStudioName, componentId);
        public Task AddComponentNode(string virtualStudioName, int componentId, float x, float y) => GetGroup(virtualStudioName).AddComponentNode(virtualStudioName, componentId, x, y);
        public Task AddInputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint) => GetGroup(virtualStudioName).AddInputToComponent(virtualStudioName, componentId, endpoint);
        public Task AddOutputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint) => GetGroup(virtualStudioName).AddOutputToComponent(virtualStudioName, componentId, endpoint);
        public Task AddPlaceholder(string virtualStudioName, StudioComponentDto component) => GetGroup(virtualStudioName).AddPlaceholder(virtualStudioName, component);
        public Task AddPlaceholderNode(string virtualStudioName, StudioComponentDto component, float x, float y) => GetGroup(virtualStudioName).AddPlaceholderNode(virtualStudioName, component, x, y);
        public Task AddPlaceholderToRepository(string virtualStudioName, StudioComponentDto component) => GetGroup(virtualStudioName).AddPlaceholderToRepository(virtualStudioName, component);
        public Task ChangeComponentProperty(string virtualStudioName, int componentId, string propertyName, object value) => GetGroup(virtualStudioName).ChangeComponentProperty(virtualStudioName, componentId, propertyName, value);
        public Task ChangeConnectionState(string virtualStudioName, int connectionId, ConnectionState state) => GetGroup(virtualStudioName).ChangeConnectionState(virtualStudioName, connectionId, state);
        public Task CreateConnection(string virtualStudioName, StudioConnectionDto connection) => GetGroup(virtualStudioName).CreateConnection(virtualStudioName, connection);
        public Task MoveComponentNode(string virtualStudioName, int componentId, float x, float y) => GetGroup(virtualStudioName).MoveComponentNode(virtualStudioName, componentId, x, y);
        public Task RemoveClientFromRepository(string virtualStudioName, int componentId) => GetGroup(virtualStudioName).RemoveClientFromRepository(virtualStudioName, componentId);
        public Task RemoveComponent(string virtualStudioName, int componentId) => GetGroup(virtualStudioName).RemoveComponent(virtualStudioName, componentId);
        public Task RemoveConnection(string virtualStudioName, int connectionId) => GetGroup(virtualStudioName).RemoveConnection(virtualStudioName, connectionId);
        public Task RemoveInputFromComponent(string virtualStudioName, int componentId, int endpointId) => GetGroup(virtualStudioName).RemoveInputFromComponent(virtualStudioName, componentId, endpointId);
        public Task RemoveOutputFromComponent(string virtualStudioName, int componentId, int endpointId) => GetGroup(virtualStudioName).RemoveOutputFromComponent(virtualStudioName, componentId, endpointId);
        public Task RemovePlaceholderFromRepository(string virtualStudioName, int componentId) => GetGroup(virtualStudioName).RemovePlaceholderFromRepository(virtualStudioName, componentId);

        private IVirtualStudioUpdateListener GetGroup(string name)
            => controlHubContext.Clients.Group(name);
    }
}
