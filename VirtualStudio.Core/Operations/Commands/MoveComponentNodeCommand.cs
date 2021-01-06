using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;

namespace VirtualStudio.Core.Operations
{
    public class MoveComponentNodeCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;
        private readonly Position2D position;

        public MoveComponentNodeCommand(int componentId, Position2D position)
        {
            this.componentId = componentId;
            this.position = position;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            if (virtualStudio is VirtualStudioArrangement studioArrangement)
            {
                ComponentNode node = studioArrangement.ComponentNodes.FirstOrDefault(c => c.Id == componentId);
                if (node is null)
                {
                    Error = new OperationError(ErrorType.NotFound, $"ComponentNode with ID {componentId} not found.");
                    return false;
                }
                node.Position = position;
                return true;
            }
            return false;
        }
    }
}
