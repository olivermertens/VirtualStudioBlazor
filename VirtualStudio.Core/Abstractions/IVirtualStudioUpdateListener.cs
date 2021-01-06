using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.DTOs;

namespace VirtualStudio.Core.Abstractions
{
    public interface IVirtualStudioUpdateListener
    {
        void MoveComponentNode(string virtualStudioName, int componentId, Position2D position);

        void AddComponent(string virtualStudioName, int componentId);
        void RemoveComponent(string virtualStudioName, int componentId);
        void AddConnection(string virtualStudioName, StudioConnectionDto connection);
        void RemoveConnection(string virtualStudioName, int connectionId);

        void ChangeComponentProperty(string virtualStudioName, int componentId, string propertyName, object value);
        void AddInputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint);
        void RemoveInputFromComponent(string virtualStudioName, int componentId, int endpointId);
        void AddOutputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint);
        void RemoveOutputFromComponent(string virtualStudioName, int componentId, int endpointId);

        void ChangeConnectionState(string virtualStudioName, int connectionId, ConnectionState state);

        void AddClientToRepository(string virtualStudioName, StudioComponentDto component);
        void RemoveClientFromRepository(string virtualStudioName, int componentId);
        void AddPlaceholderToRepository(string virtualStudioName, StudioComponentDto component);
        void RemovePlaceholderFromRepository(string virtualStudioName, int componentId);
    }
}