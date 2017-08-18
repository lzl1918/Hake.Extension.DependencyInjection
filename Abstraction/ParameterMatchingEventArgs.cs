using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class ParameterMatchingEventArgs : EventArgs
    {
        public string ParameterName { get; }
        public Type ParameterType { get; }
        public ParameterInfo ParameterInfo { get; }

        public IServiceProvider Services { get; }
        public ArgumentTraverseContext Arguments { get; }
        public IReadOnlyDictionary<string, object> Options { get; }

        public bool Handled { get; private set; }
        internal object Value { get; private set; }

        internal ParameterMatchingEventArgs(ParameterInfo parameterInfo, IServiceProvider services, IReadOnlyDictionary<string, object> options, ArgumentTraverseContext arguments)
        {
            ParameterInfo = parameterInfo;
            ParameterName = parameterInfo.Name;
            ParameterType = parameterInfo.ParameterType;
            Services = services;
            Arguments = arguments;
            Options = options;
        }

        public void SetValue(object value)
        {
            if (Handled)
                throw new Exception("cannot set value in mutiple times");
            Handled = true;
            Value = value;
        }
        internal void ClearFlags()
        {
            Handled = false;
            Value = null;
        }
    }

    public sealed class ValueMatchingEventArgs : EventArgs
    {
        public TypeInfo TargetType { get; }
        public TypeInfo InputType { get; }
        public object InputValue { get; }

        public bool Handled { get; private set; }
        internal object Value { get; private set; }

        internal ValueMatchingEventArgs(TypeInfo targetType, TypeInfo inputType, object inputValue)
        {
            TargetType = targetType;
            InputType = inputType;
            InputValue = inputValue;
        }

        public void SetValue(object value)
        {
            if (Handled)
                throw new Exception("cannot set value in mutiple times");
            Handled = true;
            Value = value;
        }
        internal void ClearFlags()
        {
            Handled = false;
            Value = null;
        }
    }
}
