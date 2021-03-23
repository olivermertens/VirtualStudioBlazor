using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ConnectionTypes.WebRtc
{
    public interface IWebRtcConnectionHandler : IConnectionHandler
    {
        event EventHandler<(int connectionId, RtcIceCandidateInit candidateJson)> IceCandidateReceived;
        Task<string> GetSdpOffer(IStudioConnection connection);
        Task<string> GetSdpAnswer(IStudioConnection connection, string sdpOffer);
        Task AddIceCandidate(IStudioConnection connection, RtcIceCandidateInit candidateJson);
        Task Connect(IStudioConnection connection, string spdAnswer);
        Task Disconnect(IStudioConnection connection);
    }
}
