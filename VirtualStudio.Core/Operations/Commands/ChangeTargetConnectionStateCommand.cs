﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Operations
{
    public class ChangeTargetConnectionStateCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        public Task<bool> Process(VirtualStudio virtualStudio) => Task.FromResult(ProcessSync(virtualStudio));

        private readonly int connectionId;
        private readonly ConnectionState state;

        public ChangeTargetConnectionStateCommand(int connectionId, ConnectionState state)
        {
            this.connectionId = connectionId;
            this.state = state;
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var connection = virtualStudio.Connections.FirstOrDefault(c => c.Id == connectionId);
            if(connection is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Connection with ID {connectionId} not found.");
                return false;
            }

            if(connection.SetTargetState(state))
                return true;
            else
            {
                Error = new OperationError(ErrorType.InvalidArgument, $"TargetState cannot be set to {state}.");
                return false;
            }
        }
    }
}
