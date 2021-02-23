using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared
{
    public enum ConnectionState
    {
        Unknown, Disconnected, Disconnecting, Connecting, Connected, Destroyed
    }
}
