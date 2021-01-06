using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public interface IStudioComponentEndpoint
    {
        int Id { get; }
        StudioComponent Component { get; }
        string Name { get; set; }
        EndpointIOType IOType { get; }
        DataKind DataKind { get; }
        string ConnectionType { get; }
    }
}
