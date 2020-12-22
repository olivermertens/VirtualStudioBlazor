using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test.DummyClasses
{
    public class DummyStudioConnectionFactory : IStudioConnectionFactory
    {
        public IStudioConnection CreateConnection(IStudioOutput output, IStudioInput input)
        {
            return new DummyStudioConnection(output, input);
        }
    }
}
