using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using System.Linq;
using VirtualStudio.Shared;
using VirtualStudio.Core.Arrangement;

namespace VirtualStudio.Core.Operations
{
    public class AddComponentNodeCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;
        private readonly float x, y;

        public AddComponentNodeCommand(int componentId, float x, float y)
        {
            this.componentId = componentId;
            this.x = x;
            this.y = y;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            if(!(virtualStudio is VirtualStudioWithArrangement virtualStudioWithArrangement))
            {
                Error = new OperationError(ErrorType.InvalidOperation, $"Operation is not supported.");
                return false;
            }

            var foundComponent = virtualStudioWithArrangement.ComponentRepository.Find(c => c.Id == componentId);
            if(foundComponent is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"StudioComponent with ID {componentId} not found in ComponentRepository.");
                return false;
            }
            if(virtualStudio.Components.Contains(foundComponent))
            {
                Error = new OperationError(ErrorType.InvalidOperation, $"The StudioComponent with ID {componentId} was already added.");
                return false;
            }

            virtualStudioWithArrangement.AddComponent(foundComponent, new Position2D(x, y));
            return true;
        }
    }
}
