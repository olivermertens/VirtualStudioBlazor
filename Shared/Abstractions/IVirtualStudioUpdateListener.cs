using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IVirtualStudioUpdateListener
    {
        Task MoveComponentNode(string virtualStudioName, int componentId, float x, float y);

        Task AddComponent(string virtualStudioName, int componentId);
        Task AddComponentNode(string virtualStudioName, int componentId, float x, float y);
        Task AddPlaceholder(string virtualStudioName, StudioComponentDto component);
        Task AddPlaceholderNode(string virtualStudioName, StudioComponentDto component, float x, float y);
        Task RemoveComponent(string virtualStudioName, int componentId);
        Task CreateConnection(string virtualStudioName, StudioConnectionDto connection);
        Task RemoveConnection(string virtualStudioName, int connectionId);

        Task ChangeComponentProperty(string virtualStudioName, int componentId, string propertyName, object value);
        Task AddInputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint);
        Task RemoveInputFromComponent(string virtualStudioName, int componentId, int endpointId);
        Task AddOutputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint);
        Task RemoveOutputFromComponent(string virtualStudioName, int componentId, int endpointId);

        Task ChangeConnectionState(string virtualStudioName, int connectionId, ConnectionState state);

        Task AddClientToRepository(string virtualStudioName, StudioComponentDto component);
        Task RemoveClientFromRepository(string virtualStudioName, int componentId);
        Task AddPlaceholderToRepository(string virtualStudioName, StudioComponentDto component);
        Task RemovePlaceholderFromRepository(string virtualStudioName, int componentId);
    }
}