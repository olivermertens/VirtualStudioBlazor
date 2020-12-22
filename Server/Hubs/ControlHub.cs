using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class ControlHub : Hub
    {

        public ControlHub()
        {

        }

        public IEnumerable<int> GetVirtualStudioIds()
        {
            yield return 1;
        }

        public VirtualStudioDto GetVirtualStudio(int id)
        {
            if(id == 1)
            {

            }
        }
    }
}