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
        public object[] Parameters { get; }
        public IReadOnlyDictionary<string, object> NamedParameters { get; }

        internal bool Handled { get; private set; }
        internal object Value { get; private set; }

        internal ParameterMatchingEventArgs(ParameterInfo parameterInfo, IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, object[] parameters)
        {
            ParameterInfo = parameterInfo;
            ParameterName = parameterInfo.Name;
            ParameterType = parameterInfo.ParameterType;
            Services = services;
            Parameters = parameters;
            NamedParameters = namedParameters;
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
