using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs.WebRtc
{
    public class ConnectWebRtcCommandArgs
    {
        public int ConnectionId { get; set; }
        public string SdpAnswer { get; set; }
        public bool UseInsertableStreams { get; set; }
    }
}
