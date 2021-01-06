using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.DTOs;

namespace VirtualStudio.Core.Operations
{
    public class AddConnectionCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly StudioConnectionDto connection;

        public AddConnectionCommand(StudioConnectionDto connection)
        {
            this.connection = connection;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var outputComponent = virtualStudio.Components.FirstOrDefault(c => c.Id == connection.OutputComponentId);
            if(outputComponent == null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find component with ID {connection.OutputComponentId}.");
                return false;
            }
            var output = outputComponent.Outputs.FirstOrDefault(o => o.Id == connection.OutputId);
            if(output is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find output with ID {connection.OutputId} on component with ID {connection.OutputComponentId}.");
                return false;
            }
            var inputComponent = virtualStudio.Components.FirstOrDefault(c => c.Id == connection.InputComponentId);
            if (inputComponent == null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find component with ID {connection.InputComponentId}.");
                return false;
            }
            var input = inputComponent.Inputs.FirstOrDefault(i => i.Id == connection.InputId);
            if (input is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find input with ID {connection.InputId} on component with ID {connection.InputComponentId}.");
                return false;
            }

            if(virtualStudio.CreateConnection(output, input) is null)
            {
                Error = new OperationError(ErrorType.InvalidOperation, $"Cannot create connection for: {connection}");
                return false;
            }
            return true;
        }
    }
}
