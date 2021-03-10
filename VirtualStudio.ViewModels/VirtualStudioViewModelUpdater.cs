using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public class VirtualStudioViewModelUpdater : IVirtualStudioUpdateListener
    {
        private readonly VirtualStudioViewModel virtualStudioViewModel;

        public VirtualStudioViewModelUpdater(VirtualStudioViewModel virtualStudioViewModel)
        {
            this.virtualStudioViewModel = virtualStudioViewModel ?? throw new ArgumentNullException(nameof(virtualStudioViewModel));
        }

        public Task AddClientToRepository(string virtualStudioName, StudioComponentDto component)
        {
            virtualStudioViewModel.ComponentRepository.Clients.Add(new ComponentViewModel(component));

            return Task.CompletedTask;
        }

        public Task AddComponent(string virtualStudioName, int componentId)
        {
            // Do nothing.
            return Task.CompletedTask;
        }

        public Task AddComponentNode(string virtualStudioName, int componentId, float x, float y)
        {
            if (!TryFindComponentInRepositoryClients(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId} in clients.");

            virtualStudioViewModel.ComponentNodes.Add(new ComponentNodeViewModel() { Component = component, PositionX = x, PositionY = y });

            return Task.CompletedTask;
        }
        public Task AddPlaceholder(string virtualStudioName, StudioComponentDto component)
        {
            // Do nothing.
            return Task.CompletedTask;
        }

        public Task AddPlaceholderNode(string virtualStudioName, StudioComponentDto component, float x, float y)
        {
            virtualStudioViewModel.ComponentNodes.Add(new ComponentNodeViewModel() { Component = new PlaceholderViewModel(component), PositionX = x, PositionY = y });

            return Task.CompletedTask;
        }

        public Task AddInputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint)
        {
            if (!TryFindComponent(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId}.");


            component.Inputs.Add(new StudioComponentEndpointViewModel(component, endpoint));

            return Task.CompletedTask;
        }

        public Task AddOutputToComponent(string virtualStudioName, int componentId, StudioComponentEndpointDto endpoint)
        {
            if (!TryFindComponent(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId}.");


            component.Outputs.Add(new StudioComponentEndpointViewModel(component, endpoint));

            return Task.CompletedTask;
        }

        public Task AddPlaceholderToRepository(string virtualStudioName, StudioComponentDto component)
        {
            virtualStudioViewModel.ComponentRepository.Placeholders.Add(new PlaceholderViewModel(component));

            return Task.CompletedTask;
        }

        public Task ChangeComponentProperty(string virtualStudioName, int componentId, string propertyName, object value)
        {
            if (!TryFindComponent(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId}.");

            switch (propertyName)
            {
                case nameof(component.Name):
                    component.Name = value as string;
                    break;
            }

            return Task.CompletedTask;
        }

        public Task ChangeConnectionState(string virtualStudioName, int connectionId, ConnectionState state)
        {
            if (!TryFindConnection(connectionId, out ConnectionViewModel connection))
                throw new Exception($"Cannot find connection with ID {connectionId}.");

            connection.State = state;

            return Task.CompletedTask;
        }

        public Task CreateConnection(string virtualStudioName, StudioConnectionDto connection)
        {
            if (!TryFindInput(connection.InputComponentId, connection.InputId, out StudioComponentEndpointViewModel input))
                throw new Exception("Connection's input not found.");

            if(!TryFindOutput(connection.OutputComponentId, connection.OutputId, out StudioComponentEndpointViewModel output))
                throw new Exception("Connection's output not found.");

            virtualStudioViewModel.Connections.Add(new ConnectionViewModel(connection.Id, input, output, connection.State));

            return Task.CompletedTask;
        }

        public Task MoveComponentNode(string virtualStudioName, int componentId, float x, float y)
        {
            if (!TryFindComponentNode(componentId, out ComponentNodeViewModel componentNode))
                throw new Exception($"Cannot find the node for component with ID {componentId}.");

            componentNode.PositionX = x;
            componentNode.PositionY = y;

            return Task.CompletedTask;
        }

        public Task RemoveClientFromRepository(string virtualStudioName, int componentId)
        {
            if(!TryFindComponentInRepositoryClients(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId} in clients.");

            if (!virtualStudioViewModel.ComponentRepository.Clients.Remove(component))
                throw new Exception($"Removing client from repository failed.");

            return Task.CompletedTask;
        }

        public Task RemovePlaceholderFromRepository(string virtualStudioName, int componentId)
        {
            if (!TryFindComponentInRepositoryPlaceholders(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId} in placeholders.");

            if (!virtualStudioViewModel.ComponentRepository.Placeholders.Remove(component as PlaceholderViewModel))
                throw new Exception($"Removing placeholder from repository failed.");

            return Task.CompletedTask;
        }

        public Task RemoveComponent(string virtualStudioName, int componentId)
        {
            if (!TryFindComponentNode(componentId, out ComponentNodeViewModel componentNode))
                throw new Exception($"Cannot find the node for component with ID {componentId}.");

            if (!virtualStudioViewModel.ComponentNodes.Remove(componentNode))
                throw new Exception($"Removing node for component with ID {componentId} failed.");

            return Task.CompletedTask;
        }

        public Task RemoveConnection(string virtualStudioName, int connectionId)
        {
            if (!TryFindConnection(connectionId, out ConnectionViewModel connection))
                throw new Exception($"Cannot find connection with ID {connectionId}.");

            if (!virtualStudioViewModel.Connections.Remove(connection))
                throw new Exception($"Removing connection with ID {connectionId} failed.");

            return Task.CompletedTask;
        }

        public Task RemoveInputFromComponent(string virtualStudioName, int componentId, int endpointId)
        {
            if(!TryFindComponent(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId}.");

            if (!TryFindInputOnComponent(component, endpointId, out StudioComponentEndpointViewModel input))
                throw new Exception($"Cannot find input with ID {endpointId} on component with ID {componentId}.");

            if (!component.Inputs.Remove(input))
                throw new Exception("Removing input failed.");

            return Task.CompletedTask;
        }

        public Task RemoveOutputFromComponent(string virtualStudioName, int componentId, int endpointId)
        {
            if (!TryFindComponent(componentId, out ComponentViewModel component))
                throw new Exception($"Cannot find component with ID {componentId}.");

            if (!TryFindOutputOnComponent(component, endpointId, out StudioComponentEndpointViewModel output))
                throw new Exception($"Cannot find output with ID {endpointId} on component with ID {componentId}.");

            if (!component.Outputs.Remove(output))
                throw new Exception("Removing output failed.");

            return Task.CompletedTask;
        }

        private bool TryFindComponent(int componentId, out ComponentViewModel component)
        {
            if (TryFindComponentInComponentNodes(componentId, out component))
                return true;
            if (TryFindComponentInRepositoryClients(componentId, out component))
                return true;
            if (TryFindComponentInRepositoryPlaceholders(componentId, out component))
                return true;
            return false;
        }

        private bool TryFindComponentNode(int componentId, out ComponentNodeViewModel componentNode)
        {
            componentNode = virtualStudioViewModel.ComponentNodes.FirstOrDefault(c => c.Component.Id == componentId);
            return componentNode != null;
        }

        private bool TryFindComponentInComponentNodes(int componentId, out ComponentViewModel component)
        {
            if (TryFindComponentNode(componentId, out ComponentNodeViewModel componentNode))
                component = componentNode.Component;
            else
                component = null;

            return component != null;
        }

        private bool TryFindComponentInRepositoryClients(int componentId, out ComponentViewModel component)
        {
            component = virtualStudioViewModel.ComponentRepository.Clients.FirstOrDefault(c => c.Id == componentId);
            return component != null;
        }

        private bool TryFindComponentInRepositoryPlaceholders(int componentId, out ComponentViewModel component)
        {
            component = virtualStudioViewModel.ComponentRepository.Placeholders.FirstOrDefault(c => c.Id == componentId);
            return component != null;
        }

        private bool TryFindConnection(int connectionId, out ConnectionViewModel connection)
        {
            connection = virtualStudioViewModel.Connections.FirstOrDefault(c => c.Id == connectionId);
            return connection != null;
        }

        private bool TryFindInputOnComponent(ComponentViewModel component, int inputId, out StudioComponentEndpointViewModel input)
        {
            input = component.Inputs.FirstOrDefault(i => i.Id == inputId);

            return input != null;
        }

        private bool TryFindOutputOnComponent(ComponentViewModel component, int outputId, out StudioComponentEndpointViewModel output)
        {
            output = component.Outputs.FirstOrDefault(o => o.Id == outputId);

            return output != null;
        }

        private bool TryFindInput(int componentId, int inputId, out StudioComponentEndpointViewModel input)
        {
            if (TryFindComponent(componentId, out ComponentViewModel component))
                TryFindInputOnComponent(component, inputId, out input);
            else
                input = null;

            return input != null;
        }

        private bool TryFindOutput(int componentId, int outputId, out StudioComponentEndpointViewModel output)
        {
            if (TryFindComponent(componentId, out ComponentViewModel component))
                TryFindOutputOnComponent(component, outputId, out output);
            else
                output = null;

            return output != null;
        }
    }
}
