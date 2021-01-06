using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Abstractions
{
    public interface IStudioConnectionFactory
    {
        string Name { get; }
        StudioConnection CreateStudioConnection(StudioComponentOutput output, StudioComponentInput input);
    }
}
