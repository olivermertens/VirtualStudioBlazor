using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class DummyClient : IStudioClient
    {
        public ClientStudioComponent GetComponent()
        {
            return new ClientStudioComponent();
        }
    }
}
