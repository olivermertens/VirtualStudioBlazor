using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Core
{
    internal class StudioConnection : IStudioConnection, IDisposable
    {
        public event EventHandler<ConnectionState> StateChanged;

        public int Id { get; }

        private ConnectionState _state;
        public ConnectionState State
        {
            get => _state;
            private set { if (value != _state) { _state = value; StateChanged?.Invoke(this, _state); } }
        }

        public ConnectionState TargetState { get; private set; }

        public StudioComponentOutput Output { get; }

        public StudioComponentInput Input { get; }

        private bool destroyed = false;
        private ConnectionState inputConnectionState = ConnectionState.Disconnected;
        private ConnectionState outputConnectionState = ConnectionState.Disconnected;

        public StudioConnection(int id, StudioComponentOutput output, StudioComponentInput input)
        {
            Id = id;
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            Output.ConnectionStateUpdated += Output_ConnectionStateUpdated;
            Input.ConnectionStateUpdated += Input_ConnectionStateUpdated;
            State = ConnectionState.Disconnected;
            TargetState = ConnectionState.Disconnected;
        }

        private void Output_ConnectionStateUpdated(object sender, (IStudioConnection connection, ConnectionState state) e)
        {
            if (e.connection == this  && e.state != outputConnectionState)
            {
                outputConnectionState = e.state;
                UpdateConnectionState();
            }
        }

        private void Input_ConnectionStateUpdated(object sender, (IStudioConnection connection, ConnectionState state) e)
        {
            if (e.connection == this && e.state != inputConnectionState)
            {
                inputConnectionState = e.state;
                UpdateConnectionState();
            }
        }

        private void UpdateConnectionState()
        {
            State = (inputConnectionState, outputConnectionState) switch
            {
                (ConnectionState.Connected, ConnectionState.Connected) => ConnectionState.Connected,
                (ConnectionState.Connected, ConnectionState.Connecting) => ConnectionState.Connecting,
                (ConnectionState.Connected, ConnectionState.Disconnecting) => ConnectionState.Disconnecting,
                (ConnectionState.Connecting, ConnectionState.Connected) => ConnectionState.Connecting,
                (ConnectionState.Connecting, ConnectionState.Connecting) => ConnectionState.Connecting,
                (ConnectionState.Connecting, ConnectionState.Disconnected) => ConnectionState.Connecting,
                (ConnectionState.Disconnected, ConnectionState.Disconnected) => ConnectionState.Disconnected,
                (ConnectionState.Disconnected, ConnectionState.Connecting) => ConnectionState.Connecting,
                (ConnectionState.Disconnected, ConnectionState.Disconnecting) => ConnectionState.Disconnecting,
                (ConnectionState.Disconnected, ConnectionState.Destroyed) => ConnectionState.Disconnected,
                (ConnectionState.Disconnecting, ConnectionState.Disconnected) => ConnectionState.Disconnecting,
                (ConnectionState.Disconnecting, ConnectionState.Disconnecting) => ConnectionState.Disconnecting,
                (ConnectionState.Disconnecting, ConnectionState.Connected) => ConnectionState.Disconnecting,
                (ConnectionState.Disconnecting, ConnectionState.Destroyed) => ConnectionState.Disconnecting,
                (ConnectionState.Destroyed, ConnectionState.Destroyed) => ConnectionState.Destroyed,
                (ConnectionState.Destroyed, ConnectionState.Disconnected) => ConnectionState.Disconnected,
                (ConnectionState.Destroyed, ConnectionState.Disconnecting) => ConnectionState.Disconnecting,
                _ => ConnectionState.Unknown
            };
        }

        private void Destroy()
        {
            if (!destroyed)
            {
                Output.ConnectionStateUpdated += Output_ConnectionStateUpdated;
                Input.ConnectionStateUpdated += Input_ConnectionStateUpdated;
                destroyed = true;
            }
        }

        public void Dispose()
        {
            SetTargetState(ConnectionState.Destroyed);
        }


        public bool SetTargetState(ConnectionState state)
        {
            if (state == TargetState)
                return true;
            if (TargetState == ConnectionState.Destroyed)
                return false;

            switch (state)
            {
                case ConnectionState.Connected:
                case ConnectionState.Disconnected:
                case ConnectionState.Destroyed:
                    TargetState = state;
                    Input.HandleConnectionTargetStateChanged(this, state);
                    if (state == ConnectionState.Destroyed)
                        Destroy();
                    return true;
                default:
                    return false;
            }
        }
    }
}
