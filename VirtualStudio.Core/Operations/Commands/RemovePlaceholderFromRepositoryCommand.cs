using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualStudio.Core.Operations
{
    public class RemovePlaceholderFromRepositoryCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;

        public RemovePlaceholderFromRepositoryCommand(int componentId)
        {
            this.componentId = componentId;
        }

        public Task<bool> Process(VirtualStudio virtualStudio) => Task.FromResult(ProcessSync(virtualStudio));

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var component = virtualStudio.ComponentRepository.Placeholders.FirstOrDefault(p => p.Id == componentId);
            if(component is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Placeholder with ID {componentId} not found.");
                return false;
            }
            if(component is PlaceholderStudioComponent placeholder)
            {
                virtualStudio.ComponentRepository.RemovePlaceholder(placeholder);
                return true;
            }
            else
            {
                Error = new OperationError(ErrorType.NotFound, $"Component with ID {componentId} is not of type PlaceholderComponent.");
                return false;
            }
        }
    }
}
