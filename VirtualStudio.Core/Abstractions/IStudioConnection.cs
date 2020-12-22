using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Abstractions
{
    public interface IStudioConnection
    {
        IStudioOutput Output { get; }
        IStudioInput Input { get; }
        ConnectionState State { get; }
        void SetTargetState(ConnectionState state);
    }
}
