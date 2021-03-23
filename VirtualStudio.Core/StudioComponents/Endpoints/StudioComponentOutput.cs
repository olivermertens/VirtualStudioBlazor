using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;

namespace VirtualStudio.Core
{
    public sealed class StudioComponentOutput : IStudioComponentEndpoint, IDisposable
    {
        public event EventHandler<(int connectionId, ConnectionState state)> ConnectionStateUpdated;

        public EndpointIOType IOType => EndpointIOType.Output;
        public int Id { get; internal set; }
        public string Name { get; set; }
        public IStudioComponent Component { get; }

        public DataKind DataKind { get; }
        public string ConnectionType { get; }

        private bool disposed = false;


        public StudioComponentOutput(int id, string name, DataKind dataKind, string connectionType, IStudioComponent component)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ConnectionType = connectionType ?? throw new ArgumentNullException(nameof(connectionType));
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Component.OutputConnectionStateUpdated += Component_OutputConnectionStateUpdated;
            DataKind = dataKind;
        }

        ~StudioComponentOutput()
        {
            Dispose();
        }

        private void Component_OutputConnectionStateUpdated(object sender, (StudioComponentOutput output, int connectionId, ConnectionState state) e)
        {
            if (e.output == this)
                ConnectionStateUpdated?.Invoke(this, (e.connectionId, e.state));
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Component.OutputConnectionStateUpdated -= Component_OutputConnectionStateUpdated;
                disposed = true;
            }
        }
    }
}
