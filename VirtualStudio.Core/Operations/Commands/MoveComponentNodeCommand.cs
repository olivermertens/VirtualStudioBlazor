using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Operations
{
    public class MoveComponentNodeCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;
        private readonly float x, y;

        public MoveComponentNodeCommand(int componentId, float x, float y)
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
            if (virtualStudio is VirtualStudioWithArrangement studioArrangement)
            {
                ComponentNode node = studioArrangement.ComponentNodes.FirstOrDefault(c => c.Id == componentId);
                if (node is null)
                {
                    Error = new OperationError(ErrorType.NotFound, $"ComponentNode with ID {componentId} not found.");
                    return false;
                }
                node.Position = new Position2D(x, y);
                return true;
            }
            return false;
        }
    }
}
