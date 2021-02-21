using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.DTOs;

namespace VirtualStudio.Core.Operations.Commands
{
    public class AddPlaceholderToRepositoryCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error => null;

        private readonly StudioComponentDto component;

        public AddPlaceholderToRepositoryCommand(StudioComponentDto component)
        {
            this.component = component;
        }

        public Task<bool> Process(VirtualStudio virtualStudio) => Task.FromResult(ProcessSync(virtualStudio));

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var placeholder = new PlaceholderStudioComponent();
            placeholder.SetName(component.Name);

            foreach(var input in component.Inputs)
            {
                placeholder.AddInput(input.Name, input.DataKind, input.ConnectionType);
            }
            foreach (var output in component.Outputs)
            {
                placeholder.AddOutput(output.Name, output.DataKind, output.ConnectionType);
            }

            return virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
        }
    }
}
