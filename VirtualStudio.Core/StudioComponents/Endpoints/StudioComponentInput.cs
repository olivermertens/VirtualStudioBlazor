using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public sealed class StudioComponentInput : IStudioComponentEndpoint, IDisposable
    {
        public event EventHandler<(IStudioConnection connection, ConnectionState state)> ConnectionStateUpdated;

        public EndpointIOType IOType => EndpointIOType.Input;
        public int Id { get; }
        public string Name { get; set; }
        public IStudioComponent Component { get; }

        public DataKind DataKind { get; }
        public string ConnectionType { get; }

        private bool disposed = false;


        public StudioComponentInput(int id, string name, DataKind dataKind, string connectionType, IStudioComponent component)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ConnectionType = connectionType ?? throw new ArgumentNullException(nameof(connectionType));
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Component.InputConnectionStateUpdated += Component_InputConnectionStateUpdated;
            DataKind = dataKind;
        }

        ~StudioComponentInput()
        {
            Dispose();
        }

        internal void HandleConnectionTargetStateChanged(IStudioConnection studioConnection, ConnectionState state)
        {
            Component.HandleConnectionTargetStateChanged(this, studioConnection, state);
        }

        private void Component_InputConnectionStateUpdated(object sender, (StudioComponentInput input, IStudioConnection connection, ConnectionState state) e)
        {
            if (e.input == this)
                ConnectionStateUpdated?.Invoke(this, (e.connection, e.state));
        }

        public void Dispose()
        {
            if(!disposed)
            {
                Component.InputConnectionStateUpdated -= Component_InputConnectionStateUpdated;
                disposed = true;
            }
        }
    }
}
