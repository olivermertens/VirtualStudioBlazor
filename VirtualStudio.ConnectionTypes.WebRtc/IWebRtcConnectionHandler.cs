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
        Task<(string sdpOffer, bool supportsInsertableStreams)> GetSdpOffer(IStudioConnection connection);
        Task<(string sdpAnwser, bool useInsertableStreams)> GetSdpAnswer(IStudioConnection connection, string sdpOffer, bool remotePeerSupportsInsertableStreams);
        Task AddIceCandidate(IStudioConnection connection, RtcIceCandidateInit candidateJson);
        Task Connect(IStudioConnection connection, string spdAnswer, bool useInsertableStreams);
        Task Disconnect(IStudioConnection connection);
    }
}
