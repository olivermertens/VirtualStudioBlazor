using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Operations
{
    public enum ErrorType
    {
        Undefined, InvalidOperation, InvalidArgument, NotFound, NotAuthorized
    }

    public class OperationError
    {
        ErrorType Type { get; }
        string Message { get; }

        public OperationError(ErrorType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}
