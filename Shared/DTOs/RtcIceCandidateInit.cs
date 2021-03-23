using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs
{
    public class RtcIceCandidateInit
    {
        public string Candidate { get; set; }
        public string SdpMid { get; set; }
        public int SdpMLineIndex { get; set; }
        public string UsernameFragment { get; set; }

        public RtcIceCandidateInit(){}

        public RtcIceCandidateInit(string candidate, string sdpMid, int sdpMLineIndex, string usernameFragment)
        {
            Candidate = candidate;
            SdpMid = sdpMid;
            SdpMLineIndex = sdpMLineIndex;
            UsernameFragment = usernameFragment;
        }
    }
}
