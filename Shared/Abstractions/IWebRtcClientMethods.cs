using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IWebRtcClientMethods
    {
        Task RequestSdpOffer(int connectionId, int endPointId, DataKind dataKind);
        Task RequestSdpAnswer(int connectionId, int endPointId, DataKind dataKind, string sdpOffer);
        Task AddIceCandidate(int connectionId, RtcIceCandidateInit candidateJson);
        Task Connect(int connectionId, string spdAnswer);
        Task Disconnect(int connectionId);
    }
}
