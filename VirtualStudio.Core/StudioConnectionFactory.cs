using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class StudioConnectionFactory : IStudioConnectionFactory
    {
        public string Name => null;

        List<IStudioConnectionFactory> connectionFactories = new List<IStudioConnectionFactory>();

        public bool RegisterStudioConnectionType(IStudioConnectionFactory connectionFactory)
        {
            if (connectionFactories.Exists(c => c.Name == connectionFactory.Name))
                return false;
            else
            {
                connectionFactories.Add(connectionFactory);
                return true;
            }
        }

        public bool CanCreateStudioConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            if (output.ConnectionType == input.ConnectionType && output.DataKind == input.DataKind)
                return connectionFactories.Exists(c => c.Name == output.ConnectionType);
            else
                return false;
        }

        public StudioConnection CreateStudioConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            if (!CanCreateStudioConnection(output, input))
                return null;

            IStudioConnectionFactory connectionFactory = connectionFactories.First(c => c.Name == output.ConnectionType);
            return connectionFactory.CreateStudioConnection(output, input);
        }
    }
}
