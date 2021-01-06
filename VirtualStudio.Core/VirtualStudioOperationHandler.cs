using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.DTOs;
using VirtualStudio.Core.Operations;

namespace VirtualStudio.Core
{
    public class VirtualStudioOperationHandler : IVirtualStudioUpdateListener
    {
        private readonly VirtualStudio virtualStudio;
        private readonly string virtualStudioName;

        public VirtualStudioOperationHandler(VirtualStudio virtualStudio, string virtualStudioName)
        {
            this.virtualStudio = virtualStudio ?? throw new ArgumentNullException(nameof(virtualStudio));
            this.virtualStudioName = virtualStudioName ?? throw new ArgumentNullException(nameof(virtualStudioName));
        }

        #region IVirtualStudioOperationHandler

        private void ExecuteCommand(string virtualStudioName, IVirtualStudioOperation command)
        {
            if(virtualStudioName == this.virtualStudioName)
            {

            }
        }

        public void MoveComponentNode(string virtualStudioName, int componentId, Position2D position)
            => ExecuteCommand(virtualStudioName, new MoveComponentNodeCommand(componentId, position));

        public void AddComponent(string virtualStudioName, int componentId)
            => ExecuteCommand(virtualStudioName, new AddComponentCommand(componentId));

        public void RemoveComponent(string virtualStudioName, int componentId)
            => ExecuteCommand(virtualStudioName, new RemoveComponentCommand(componentId));

        public void AddConnection(string virtualStudioName, StudioConnectionDto connection)
            => ExecuteCommand(virtualStudioName, new AddConnectionCommand(connection));

        public void RemoveConnection(string virtualStudioName, int connectionId)
            => ExecuteCommand(virtualStudioName, new RemoveConnectionCommand(connectionId));

        public void ChangeComponentProperty(string virtualStudioName, int componentId, string propertyName, object value)
            => ExecuteCommand(virtualStudioName, new ChangeComponentPropertyCommand(componentId, propertyName, value));

        public void AddInputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint)
            => ExecuteCommand(virtualStudioName, new AddInputToPlaceholderCommand(componentId, endpoint));

        public void RemoveInputFromComponent(string virtualStudioName, int componentId, int endpointId)
            => ExecuteCommand(virtualStudioName, new RemoveInputFromPlaceholderComponentCommand(componentId, endpointId));

        public void AddOutputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint)
            => ExecuteCommand(virtualStudioName, new AddOutputToPlaceholderCommand(componentId, endpoint));

        public void RemoveOutputFromComponent(string virtualStudioName, int componentId, int endpointId)
            => ExecuteCommand(virtualStudioName, new RemoveOutputFromPlaceholderComponentCommand(componentId, endpointId));

        public void ChangeConnectionState(string virtualStudioName, int connectionId, ConnectionState state)
        {
            throw new NotImplementedException();
        }

        public void AddClientToRepository(string virtualStudioName, StudioComponentDto component)
        {
            if(virtualStudioName == this.virtualStudioName)           
                AddClientToRepository(component);   
        }

        public void RemoveClientFromRepository(string virtualStudioName, int componentId)
        {
            throw new NotImplementedException();
        }

        public void AddPlaceholderToRepository(string virtualStudioName, StudioComponentDto component)
        {
            throw new NotImplementedException();
        }

        public void RemovePlaceholderFromRepository(string virtualStudioName, int componentId)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void MoveComponentNode(int componentId, Position2D position)
        {
            throw new NotImplementedException();
        }

        public void AddClientToRepository(StudioComponentDto component)
        {

        }
    }
}
