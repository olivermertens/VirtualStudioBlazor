using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IWebRtcHubMethods
    {
        Task RespondSdpOffer(int connectionId, string sdpOffer, bool supportsInsertableStreams);
        Task RespondSdpAnswer(int connectionId, string sdpAnswer, bool useInsertableStreams);
        Task SendIceCandidate(int connectionId, RtcIceCandidateInit candidateJson);
    }
}
