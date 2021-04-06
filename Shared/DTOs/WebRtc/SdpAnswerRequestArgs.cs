using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs.WebRtc
{
    public class SdpAnswerRequestArgs
    {
        public int ConnectionId { get; set; }
        public int EndPointId { get; set; }
        public DataKind DataKind { get; set; }
        public string SdpOffer { get; set; }
        public bool RemotePeerSupportsInsertableStreams { get; set; }
    }
}
