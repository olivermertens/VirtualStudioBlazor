using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Shared;

namespace VirtualStudio.Core
{
    public interface IStudioConnection
    {
        event EventHandler<ConnectionState> StateChanged;

        int Id { get; }
        ConnectionState State { get; }
        ConnectionState TargetState { get; }
        StudioComponentOutput Output { get; }
        StudioComponentInput Input { get; }
        bool SetTargetState(ConnectionState state);
    }
}
