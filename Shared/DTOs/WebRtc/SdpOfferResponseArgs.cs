using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs.WebRtc
{
    public class SdpOfferResponseArgs
    {
        public int ConnectionId { get; set; }
        public string SdpOffer { get; set; }
        public bool SupportsInsertableStreams { get; set; }
    }
}
