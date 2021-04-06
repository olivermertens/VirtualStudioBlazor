using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IWebRtcSignalingServer
    {
        Task SendSdpOffer(SdpOfferResponseArgs args);
        Task SendSdpAnswer(SdpAnswerResponseArgs args);
        Task SendIceCandidate(IceCandidateArgs args);
    }
}
