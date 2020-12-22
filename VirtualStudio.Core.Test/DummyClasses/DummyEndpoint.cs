using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test.DummyClasses
{
    public class DummyEndpoint : IStudioEndpoint, IStudioInput, IStudioOutput
    {
        public DataKind DataKind => throw new NotImplementedException();
    }
}
