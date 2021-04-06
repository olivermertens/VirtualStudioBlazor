using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IWebRtcClient
    {
        Task RequestSdpOffer(SdpOfferRequestArgs args);
        Task RequestSdpAnswer(SdpAnswerRequestArgs args);
        Task AddIceCandidate(IceCandidateArgs args);
        Task Connect(ConnectWebRtcCommandArgs args);
        Task Disconnect(DisconnectWebRtcCommandArgs args);
    }
}
