using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VirtualStudio.Core.Client.Abstractions
{
    public interface IStudioClientConnection
    {
        Func<IStudioClientMessage<object>, Task<object>> MessageReceivedHandler { set; }
        Task<T> SendMessageAsync<T>(IStudioClientMessage<T> message);
    }
}
