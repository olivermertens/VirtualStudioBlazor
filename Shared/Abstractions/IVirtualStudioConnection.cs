using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IVirtualStudioConnection : IStudioClientUpdater, IWebRtcSignalingServer
    {
        event EventHandler<SdpOfferRequestArgs> SdpOfferRequestReceived;
        event EventHandler<SdpAnswerRequestArgs> SdpAnswerRequestReceived;
        event EventHandler<IceCandidateArgs> IceCandidateReceived;
        event EventHandler<ConnectWebRtcCommandArgs> ConnectCommandReceived;
        event EventHandler<DisconnectWebRtcCommandArgs> DisconnectCommandReceived;

        Task<bool> JoinVirtualStudio(string virtualStudioName, StudioComponentDto studioComponent);
        Task<bool> LeaveVirtualStudio(string virtualStudioName);

        void AddListener(IWebRtcClient webRtcClient);
        void RemoveListener(IWebRtcClient webRtcClient);
        void ClearListeners();

    }
}