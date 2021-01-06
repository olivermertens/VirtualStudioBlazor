using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Operations
{
    public class AddComponentCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;

        public AddComponentCommand(int componentId)
        {
            this.componentId = componentId;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var foundComponent = virtualStudio.ComponentRepository.Find(c => c.Id == componentId);
            if(foundComponent is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"StudioComponent with ID {componentId} not found in ComponentRepository.");
                return false;
            }
            virtualStudio.AddComponent(foundComponent);
            return true;
        }
    }
}
