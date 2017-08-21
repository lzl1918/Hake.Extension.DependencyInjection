using System;

namespace Hake.Extension.DependencyInjection.Abstraction.Internals
{
    internal sealed class InvokeMethodResult
    {
        public bool IsExecuted { get; }
        public object ReturnValue { get; }
        public Type ReturnType { get; }
        public Exception Exception { get; }

        public bool IsExecutionSucceeded { get { return IsExecuted && Exception == null; } }
        public bool HasReturnValueBySignature { get { return ReturnType != typeof(void); } }

        private InvokeMethodResult(bool isExecuted, object returnValue, Type returnType, Exception exception)
        {
            IsExecuted = isExecuted;
            ReturnValue = returnValue;
            ReturnType = returnType;
            Exception = exception;
        }
        internal static InvokeMethodResult Failed(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new InvokeMethodResult(true, null, null, exception);
        }
        internal static InvokeMethodResult Success(Type returnType, object returnValue)
        {
            if (returnType == null)
                throw new ArgumentNullException(nameof(returnType));

            return new InvokeMethodResult(true, returnValue, returnType, null);
        }
        internal static InvokeMethodResult MethodNotFound()
        {
            return new InvokeMethodResult(false, null, null, null);
        }
    }
}
