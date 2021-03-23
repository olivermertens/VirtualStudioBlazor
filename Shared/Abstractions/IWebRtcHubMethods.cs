using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IWebRtcHubMethods
    {
        Task RespondSdpOffer(int connectionId, string sdpOffer);
        Task RespondSdpAnswer(int connectionId, string sdpAnswer);
        Task SendIceCandidate(int connectionId, RtcIceCandidateInit candidateJson);
    }
}
