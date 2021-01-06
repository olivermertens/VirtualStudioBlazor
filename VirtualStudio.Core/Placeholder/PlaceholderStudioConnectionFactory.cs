using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class PlaceholderStudioConnectionFactory : IStudioConnectionFactory
    {
        public static string Name => "Placeholder";
        string IStudioConnectionFactory.Name => Name;

        public StudioConnection CreateStudioConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            var connection = new PlaceholderStudioConnection() { Output = output, Input = input };
            connection.SetTargetState(ConnectionState.Disconnected);
            return connection;
        }
    }
}
