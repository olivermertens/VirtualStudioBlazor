using System;
using System.Collections.ObjectModel;
using System.Linq;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public class VirtualStudioViewModel
    {
        public StudioComponentRepositoryViewModel ComponentRepository { get; set; }
        public ObservableCollection<ComponentNodeViewModel> ComponentNodes { get; } = new ObservableCollection<ComponentNodeViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

        public VirtualStudioViewModel(VirtualStudioWithArrangementDto virtualStudioDto)
        {
            ComponentRepository = new StudioComponentRepositoryViewModel(virtualStudioDto.ComponentRepository);

            foreach (var node in virtualStudioDto.ComponentNodes)
            {
                ComponentViewModel component;
                if (node.Component.IsPlaceholder)
                    component = new PlaceholderViewModel(node.Component);
                else
                    component = ComponentRepository.Clients.First(c => c.Id == node.Component.Id);

                ComponentNodes.Add(new ComponentNodeViewModel
                {
                    Component = component,
                    PositionX = node.X,
                    PositionY = node.Y
                });
            }

            foreach (var connection in virtualStudioDto.Connections)
            {
                var input = FindInputInComponentNodes(connection.InputComponentId, connection.InputId);
                var output = FindOutputInComponentNodes(connection.OutputComponentId, connection.OutputId);
                Connections.Add(new ConnectionViewModel(connection.Id, input, output, connection.State));
            }
        }

        private StudioComponentEndpointViewModel FindInputInComponentNodes(int inputComponentId, int inputId)
            =>ComponentNodes.First(c => c.Component.Id == inputComponentId).Component.Inputs.First(i => i.Id == inputId);

        private StudioComponentEndpointViewModel FindOutputInComponentNodes(int outputComponentId, int outputId)
           => ComponentNodes.First(c => c.Component.Id == outputComponentId).Component.Outputs.First(i => i.Id == outputId);
    }
}