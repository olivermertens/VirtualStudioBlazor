using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Abstractions
{
    public abstract class StudioConnection
    {
        public virtual event EventHandler<ConnectionState> StateChanged;

        public int Id { get; internal set; }
        public ConnectionState State { get; private set; }
        public ConnectionState TargetState { get; private set; }

        public StudioComponentOutput Output { get; internal set; }
        public StudioComponentInput Input { get; internal set; }


        public StudioConnection()
        {
            State = ConnectionState.Disconnected;
        }

        public virtual void SetTargetState(ConnectionState state)
        {
            TargetState = state;
        }

        protected void SetState(ConnectionState state)
        {
            if (state != State)
            {
                State = state;
                StateChanged?.Invoke(this, State);
            }
        }
    }
}
