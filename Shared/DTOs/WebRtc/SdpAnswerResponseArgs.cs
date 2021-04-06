using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs.WebRtc
{
    public class SdpAnswerResponseArgs
    {
        public int ConnectionId { get; set; }
        public string SdpAnswer { get; set; }
        public bool UseInsertableStreams { get; set; }
    }
}
