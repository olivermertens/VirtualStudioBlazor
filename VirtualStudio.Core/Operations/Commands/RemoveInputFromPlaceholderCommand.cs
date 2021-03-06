﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Operations
{
    public class RemoveInputFromPlaceholderCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;
        private readonly int endpointId;

        public RemoveInputFromPlaceholderCommand(int componentId, int endpointId)
        {
            this.componentId = componentId;
            this.endpointId = endpointId;
        }

        public Task<bool> Process(VirtualStudio virtualStudio) => Task.FromResult(ProcessSync(virtualStudio));

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var component = virtualStudio.FindStudioComponentById(componentId);
            if (component is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Component with ID {componentId} not found.");
                return false;
            }
            if (component is PlaceholderStudioComponent placeholderStudioComponent)
            {
                var input = placeholderStudioComponent.Inputs.FirstOrDefault(i => i.Id == endpointId);
                if (input is null)
                {
                    Error = new OperationError(ErrorType.NotFound, $"Input with ID {endpointId} not found on component with ID {componentId}.");
                    return false;
                }
                placeholderStudioComponent.RemoveInput(input);
                return true;
            }
            else
            {
                Error = new OperationError(ErrorType.InvalidOperation, $"Component with ID {componentId} is not of type PlaceholderComponent.");
                return false;
            }
        }
    }
}
