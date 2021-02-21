using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.DTOs;

namespace VirtualStudio.Core
{
    public class VirtualStudioEventProcessor : IDisposable
    {
        public string VirtualStudioName { get; }

        private readonly VirtualStudio virtualStudio;
        private readonly IVirtualStudioUpdateListener operationHandler;

        public VirtualStudioEventProcessor(VirtualStudio virtualStudio, string virtualStudioName, IVirtualStudioUpdateListener operationHandler)
        {
            this.virtualStudio = virtualStudio ?? throw new ArgumentNullException(nameof(virtualStudio));
            VirtualStudioName = virtualStudioName ?? throw new ArgumentNullException(nameof(virtualStudioName));
            this.operationHandler = operationHandler ?? throw new ArgumentNullException(nameof(operationHandler));

            if (virtualStudio is VirtualStudioArrangement virtualStudioArrangement)
            {
                virtualStudioArrangement.ComponentNodeMoved += VirtualStudioArrangement_ComponentNodeMoved;
            }

            virtualStudio.ComponentAdded += VirtualStudio_ComponentAdded;
            virtualStudio.ComponentRemoved += VirtualStudio_ComponentRemoved;
            virtualStudio.ConnectionAdded += VirtualStudio_ConnectionAdded;
            virtualStudio.ConnectionRemoved += VirtualStudio_ConnectionRemoved;

            virtualStudio.ComponentRepository.ClientAdded += ComponentRepository_ClientAdded;
            virtualStudio.ComponentRepository.ClientRemoved += ComponentRepository_ClientRemoved;
            virtualStudio.ComponentRepository.PlaceholderAdded += ComponentRepository_PlaceholderAdded;
            virtualStudio.ComponentRepository.PlaceholderRemoved += ComponentRepository_PlaceholderRemoved;
        }

        public void Dispose()
        {
            if (virtualStudio is VirtualStudioArrangement virtualStudioArrangement)
            {
                virtualStudioArrangement.ComponentNodeMoved -= VirtualStudioArrangement_ComponentNodeMoved;
            }

            virtualStudio.ComponentAdded -= VirtualStudio_ComponentAdded;
            virtualStudio.ComponentRemoved -= VirtualStudio_ComponentRemoved;
            virtualStudio.ConnectionAdded -= VirtualStudio_ConnectionAdded;
            virtualStudio.ConnectionRemoved -= VirtualStudio_ConnectionRemoved;

            virtualStudio.ComponentRepository.ClientAdded -= ComponentRepository_ClientAdded;
            virtualStudio.ComponentRepository.ClientRemoved -= ComponentRepository_ClientRemoved;
            virtualStudio.ComponentRepository.PlaceholderAdded -= ComponentRepository_PlaceholderAdded;
            virtualStudio.ComponentRepository.PlaceholderRemoved -= ComponentRepository_PlaceholderRemoved;
        }

        private void AttachComponentEventHandlers(IStudioComponent component)
        {
            component.PropertyChanged += Component_PropertyChanged;
            component.InputAdded += Component_InputAdded;
            component.InputRemoved += Component_InputRemoved;
            component.OutputAdded += Component_OutputAdded;
            component.OutputRemoved += Component_OutputRemoved;
        }

        private void DetachComponentEventHandlers(IStudioComponent component)
        {
            component.PropertyChanged -= Component_PropertyChanged;
            component.InputAdded -= Component_InputAdded;
            component.InputRemoved -= Component_InputRemoved;
            component.OutputAdded -= Component_OutputAdded;
            component.OutputRemoved -= Component_OutputRemoved;
        }


        #region EventHandlers

        #region VirtualStudioArragenment
        private void VirtualStudioArrangement_ComponentNodeMoved(object sender, ComponentNode componentNode)
        {
            operationHandler.MoveComponentNode(VirtualStudioName, componentNode.Id, componentNode.Position);
        }
        #endregion

        #region VirtualStudio
        private void VirtualStudio_ComponentAdded(object sender, IStudioComponent component)
        {
            AttachComponentEventHandlers(component);
            operationHandler.AddComponent(VirtualStudioName, component.Id);
        }

        private void VirtualStudio_ComponentRemoved(object sender, IStudioComponent component)
        {
            DetachComponentEventHandlers(component);
            operationHandler.RemoveComponent(VirtualStudioName, component.Id);
        }

        private void VirtualStudio_ConnectionAdded(object sender, IStudioConnection connection)
        {
            connection.StateChanged += Connection_StateChanged;
            operationHandler.AddConnection(VirtualStudioName, connection.ToDto());
        }

        private void VirtualStudio_ConnectionRemoved(object sender, IStudioConnection connection)
        {
            connection.StateChanged -= Connection_StateChanged;
            operationHandler.RemoveConnection(VirtualStudioName, connection.Id);
        }

        #endregion

        #region ComponentRepository

        private void ComponentRepository_ClientAdded(object sender, IStudioComponent client)
        {
            AttachComponentEventHandlers(client);
            operationHandler.AddClientToRepository(VirtualStudioName, client.ToDto());
        }

        private void ComponentRepository_ClientRemoved(object sender, IStudioComponent client)
        {
            DetachComponentEventHandlers(client);
            operationHandler.RemoveClientFromRepository(VirtualStudioName, client.Id);
        }

        private void ComponentRepository_PlaceholderAdded(object sender, IStudioComponent placeholder)
        {
            AttachComponentEventHandlers(placeholder);
            operationHandler.AddPlaceholderToRepository(VirtualStudioName, placeholder.ToDto());
        }

        private void ComponentRepository_PlaceholderRemoved(object sender, IStudioComponent placeholder)
        {
            DetachComponentEventHandlers(placeholder);
            operationHandler.RemovePlaceholderFromRepository(VirtualStudioName, placeholder.Id);
        }

        #endregion

        #region Component

        private void Component_InputAdded(object sender, StudioComponentInput input)
        {
            operationHandler.AddInputToComponent(VirtualStudioName, input.Component.Id, input.ToDto());
        }

        private void Component_InputRemoved(object sender, StudioComponentInput input)
        {
            operationHandler.RemoveInputFromComponent(VirtualStudioName, input.Component.Id, input.Id);
        }

        private void Component_OutputAdded(object sender, StudioComponentOutput output)
        {
            operationHandler.AddOutputToComponent(VirtualStudioName, output.Component.Id, output.ToDto());
        }

        private void Component_OutputRemoved(object sender, StudioComponentOutput output)
        {
            operationHandler.RemoveOutputFromComponent(VirtualStudioName, output.Component.Id, output.Id);
        }

        private void Component_PropertyChanged(object sender, string propertyName)
        {
            if(sender is IStudioComponent component)
            {
                switch(propertyName)
                {
                    case nameof(component.Name):
                        operationHandler.ChangeComponentProperty(VirtualStudioName, component.Id, propertyName, component.Name);
                        break;
                }
            }
        }

        #endregion

        #region Connection

        private void Connection_StateChanged(object sender, ConnectionState connectionState)
        {
            if (sender is StudioConnection connection)
            {
                operationHandler.ChangeConnectionState(VirtualStudioName, connection.Id, connection.State);
            }
        }

        #endregion

        #endregion
    }
}
