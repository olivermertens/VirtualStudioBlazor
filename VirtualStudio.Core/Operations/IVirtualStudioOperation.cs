using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Operations
{
    public interface IVirtualStudioOperation
    {
        OperationError Error { get; }
    }

    public interface IVirtualStudioOperation<T> : IVirtualStudioOperation
    {
        Task<T> Process(VirtualStudio virtualStudio);
    }
}
