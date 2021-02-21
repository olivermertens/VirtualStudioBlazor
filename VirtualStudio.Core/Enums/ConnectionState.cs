using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core
{
    public enum ConnectionState
    {
        Unknown, Disconnected, Disconnecting, Connecting, Connected, Destroyed
    }
}
