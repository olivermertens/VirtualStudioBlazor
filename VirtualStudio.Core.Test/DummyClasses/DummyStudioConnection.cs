using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test.DummyClasses
{
    public class DummyStudioConnection : IStudioConnection
    {
        public ConnectionState State => throw new NotImplementedException();
        public IStudioOutput Output { get; }
        public IStudioInput Input { get; }

        public DummyStudioConnection(IStudioOutput output, IStudioInput input)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public void SetTargetState(ConnectionState state)
        {
            throw new NotImplementedException();
        }
    }
}
