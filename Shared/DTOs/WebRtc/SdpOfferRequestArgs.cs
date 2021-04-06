using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs.WebRtc
{
    public class SdpOfferRequestArgs
    {
        public int ConnectionId { get; set; }
        public int EndPointId { get; set; }
        public DataKind DataKind { get; set; }
    }
}
