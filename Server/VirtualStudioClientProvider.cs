using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.ConnectionTypes.WebRtc;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.StudioClient;

namespace VirtualStudio.Server
{
    public class VirtualStudioClientProvider
    {
        IHubContext<VirtualStudioHub, IWebRtcClient> virtualStudioHubContext;

        public VirtualStudioClientProvider(IHubContext<VirtualStudioHub, IWebRtcClient> virtualStudioHubContext)
        {
            this.virtualStudioHubContext = virtualStudioHubContext;
        }

        public IWebRtcClient GetSignalRClient(string connectionId)
        {
            return virtualStudioHubContext.Clients.Client(connectionId);
        }
    }
}
