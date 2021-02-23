using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Core.Operations;

namespace VirtualStudio.Core.Operations
{
    public enum OperationStatus
    {
        Success, Error
    }

    public class OperationResponse
    {
        public OperationStatus Status { get; set; }
        public OperationError Error { get; set; }
        public OperationResponse() { }

        public OperationResponse(OperationError error)
        {
            Error = error;
            Status = Error is null ? OperationStatus.Success : OperationStatus.Error;
        }
    }

    public class OperationResponse<T> : OperationResponse
    {
        public T Data { get; set; }

        public OperationResponse() :base() { }

        public OperationResponse(OperationError error) : base(error) { }

        public OperationResponse(T data)
        {
            Status = OperationStatus.Success;
            Data = data;
        }

        public OperationResponse WithoutData()
        {
            return new OperationResponse(Error);
        }
    }
}
