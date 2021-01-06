using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public sealed class StudioComponentOutput : IStudioComponentEndpoint
    {
        public EndpointIOType IOType => EndpointIOType.Output;
        public int Id { get; internal set; }
        public string Name { get; set; }
        public StudioComponent Component { get; internal set; }
        public DataKind DataKind { get; }
        public string ConnectionType { get; }

        public StudioComponentOutput(string name, DataKind dataKind, string connectionType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DataKind = dataKind;
            ConnectionType = connectionType ?? throw new ArgumentNullException(nameof(connectionType));
        }
    }
}
