using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs.WebRtc
{
    public class IceCandidateArgs
    {
        public int ConnectionId { get; set; }
        public RtcIceCandidateDto CandidateDto { get; set; }
    }
}
