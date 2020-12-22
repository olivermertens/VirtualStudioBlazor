using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Abstractions
{
    public interface IStudioConnectionFactory
    {
        IStudioConnection CreateConnection(IStudioOutput output, IStudioInput input);
    }
}
