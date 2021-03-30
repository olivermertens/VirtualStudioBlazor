using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class StudioConnectionFactory
    {
        private IdGenerator idGenerator = new IdGenerator();

        public bool CanCreateStudioConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            return
                (output.Component != null) && (input.Component != null) &&
                input.ConnectionType == output.ConnectionType &&
                (input.DataKind & output.DataKind) != Shared.DataKind.Nothing;
        }

        public IStudioConnection CreateStudioConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            if (!CanCreateStudioConnection(output, input))
                return null;

            return new StudioConnection(idGenerator.GetNewId(), output, input);
        }
    }
}
