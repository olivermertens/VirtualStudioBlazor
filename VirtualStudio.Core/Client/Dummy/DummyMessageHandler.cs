using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Client.Abstractions;

namespace VirtualStudio.Core.Client.Dummy
{
    public class DoSomethingClientCommand : IStudioClientMessage<bool> { }

    public class DummyMessageHandler : IStudioClientMessageHandler<DoSomethingClientCommand, bool>
    {
        public Task<bool> HandleMessageAsync(DoSomethingClientCommand command)
        {
            return Task.FromResult(true);
        }
    }
}
