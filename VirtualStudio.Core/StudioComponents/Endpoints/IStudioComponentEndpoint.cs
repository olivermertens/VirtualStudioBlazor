using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;

namespace VirtualStudio.Core
{
    public interface IStudioComponentEndpoint
    {
        event EventHandler<(IStudioConnection connection, ConnectionState state)> ConnectionStateUpdated;
        int Id { get; }
        IStudioComponent Component { get; }
        string Name { get; set; }
        EndpointIOType IOType { get; }
        DataKind DataKind { get; }
        string ConnectionType { get; }
    }
}
