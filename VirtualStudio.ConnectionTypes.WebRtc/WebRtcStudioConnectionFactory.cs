using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.ConnectionTypes.WebRtc
{
    public class WebRtcStudioConnectionFactory : IStudioConnectionFactory
    {
        public static string Name => "WebRTC";
        string IStudioConnectionFactory.Name => Name;

        public StudioConnection CreateStudioConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            return new WebRtcStudioConnection();
        }
    }
}
