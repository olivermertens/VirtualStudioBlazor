﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Core.Operations
{
    public class AddOutputToPlaceholderCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;
        private readonly StudioComponentEndpointDto endpoint;

        public AddOutputToPlaceholderCommand(int componentId, StudioComponentEndpointDto endpoint)
        {
            this.componentId = componentId;
            this.endpoint = endpoint;
        }

        public Task<bool> Process(VirtualStudio virtualStudio) => Task.FromResult(ProcessSync(virtualStudio));

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var component = virtualStudio.FindStudioComponentById(componentId);
            if (component is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"StudioComponent with ID {componentId} not found.");
                return false;
            }
            if (component is PlaceholderStudioComponent placeholderComponent)
            {
                placeholderComponent.AddOutput(endpoint.Name, endpoint.DataKind, endpoint.ConnectionType);
                return true;
            }
            else
            {
                Error = new OperationError(ErrorType.NotFound, $"StudioComponent with ID {component} is not a Placeholder.");
                return false;
            }
        }
    }
}
