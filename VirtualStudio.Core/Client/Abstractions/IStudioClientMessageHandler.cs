using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Client.Abstractions
{
    public interface IStudioClientMessageHandler { }

    public interface IStudioClientMessageHandler<Cmd, TReturn> : IStudioClientMessageHandler where Cmd : IStudioClientMessage<TReturn>
    {
        Task<TReturn> HandleMessageAsync(Cmd command);
    }
}
