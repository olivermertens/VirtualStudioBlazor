﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.DTOs;

namespace VirtualStudio.Core.Operations
{
    public class AddInputToPlaceholderCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;
        private readonly StudioComponentEndpointDto endpoint;

        public AddInputToPlaceholderCommand(int componentId, StudioComponentEndpointDto endpoint)
        {
            this.componentId = componentId;
            this.endpoint = endpoint;
        }

        public Task<bool> Process(VirtualStudio virtualStudio) => Task.FromResult(ProcessSync(virtualStudio));

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var component = virtualStudio.Components.FirstOrDefault(c => c.Id == componentId);
            if(component is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Component with ID {componentId} not found.");
                return false;
            }
            if(component is PlaceholderStudioComponent placeholderComponent)
            {
                var input = new StudioComponentInput(endpoint.Name, endpoint.DataKind, endpoint.ConnectionType);
                placeholderComponent.AddInput(input);
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
