using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared
{
    public enum ErrorType
    {
        Undefined, InvalidOperation, InvalidArgument, NotFound, NotAuthorized, Disconnected
    }

    public class OperationError
    {
        public ErrorType Type { get; }
        public string Message { get; }

        public OperationError(ErrorType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}
