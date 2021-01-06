using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class PlaceholderStudioConnection : StudioConnection
    {
        public PlaceholderStudioConnection()
        {
            SetState(ConnectionState.Disconnected);
        }

        public override void SetTargetState(ConnectionState state)
        {
            base.SetTargetState(state);
        }
    }
}
