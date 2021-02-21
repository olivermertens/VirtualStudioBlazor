using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;

namespace VirtualStudio.Core.Operations
{
    public class RemoveComponentCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;

        public RemoveComponentCommand(int componentId)
        {
            this.componentId = componentId;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            IStudioComponent studioComponent = virtualStudio.Components.FirstOrDefault(c => c.Id == componentId);
            if(studioComponent is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Component with ID {componentId} not found.");
                return false;
            }
            virtualStudio.RemoveComponent(studioComponent);
            return true;
        }
    }
}
