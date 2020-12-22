using System.Threading.Tasks;

namespace VirtualStudio.Shared
{
    public interface IVirtualStudioClient
    {
        void ReceiveMessage(string message);
        bool InitWebRtcConnection(int linkId, EndpointDescription sendingEndpoint);
        string GetSdpAnswer(string sdpOffer, EndpointDescription receivingEndpoint);
        Task DisconnectLink(int linkId);
    }
}