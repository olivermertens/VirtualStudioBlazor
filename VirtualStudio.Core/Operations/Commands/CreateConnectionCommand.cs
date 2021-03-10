using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Operations
{
    public class CreateConnectionCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int outputComponentId;
        private readonly int outputId;
        private readonly int inputComponentId;
        private readonly int inputId;

        public CreateConnectionCommand(int outputComponentId, int outputId, int inputComponentId, int inputId)
        {
            this.outputComponentId = outputComponentId;
            this.outputId = outputId;
            this.inputComponentId = inputComponentId;
            this.inputId = inputId;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var inputComponent = virtualStudio.Components.FirstOrDefault(c => c.Id == inputComponentId);
            if (inputComponent == null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find component with ID {inputComponentId}.");
                return false;
            }
            var input = inputComponent.Inputs.FirstOrDefault(i => i.Id == inputId);
            if (input is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find input with ID {inputId} on component with ID {inputComponentId}.");
                return false;
            }
            if(virtualStudio.Connections.FirstOrDefault(c => c.Input == input) != null)
            {
                Error = new OperationError(ErrorType.InvalidOperation, $"Input with ID {inputId} on component with ID {inputComponentId} is already connected.");
                return false;
            }
            var outputComponent = virtualStudio.Components.FirstOrDefault(c => c.Id == outputComponentId);
            if(outputComponent == null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find component with ID {outputComponentId}.");
                return false;
            }
            var output = outputComponent.Outputs.FirstOrDefault(o => o.Id == outputId);
            if(output is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Cannot find output with ID {outputId} on component with ID {outputComponentId}.");
                return false;
            }
            if(output.DataKind != input.DataKind)
            {
                Error = new OperationError(ErrorType.InvalidOperation, "DataKind of output and input does not match.");
                return false;
            }
            if(output.ConnectionType != input.ConnectionType)
            {
                Error = new OperationError(ErrorType.InvalidOperation, "ConnectionType of output and input does not match.");
                return false;
            }

            if(virtualStudio.CreateConnection(output, input) is null)
            {
                Error = new OperationError(ErrorType.InvalidOperation, $"Cannot create connection for: Output {outputComponentId}:{outputId}, Input {inputComponentId}:{inputId}");
                return false;
            }
            return true;
        }
    }
}
