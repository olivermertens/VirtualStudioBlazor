using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class ClientStudioComponent : StudioComponent
    {
        IStudioClient Client { get; }
    }
}
