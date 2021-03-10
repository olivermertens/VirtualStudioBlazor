using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Operations
{
    public class RemoveConnectionCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int connectionId;

        public RemoveConnectionCommand(int connectionId)
        {
            this.connectionId = connectionId;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var connection = virtualStudio.Connections.FirstOrDefault(c => c.Id == connectionId);
            if(connection is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Connection with ID {connectionId} not found.");
                return false;
            }
            virtualStudio.RemoveConnection(connection);
            return true;
        }
    }
}
