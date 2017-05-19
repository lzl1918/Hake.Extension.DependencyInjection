using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class InvokeMethodResult
    {
        public bool IsExecuted { get; }
        public object ReturnValue { get; }
        public Type ReturnType { get; }
        public Exception Exception { get; }

        public bool IsExecutionSucceeded { get { return IsExecuted && Exception == null; } }
        public bool HasReturnValueBySignature { get { return ReturnType != typeof(void); } }

        internal InvokeMethodResult(bool isExecuted, object returnValue, Type returnType, Exception exception)
        {
            IsExecuted = isExecuted;
            ReturnValue = returnValue;
            ReturnType = returnType;
            Exception = exception;
        }
    }
}
