using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class MatchingParameterEventArgs : EventArgs
    {
        public string ParameterName { get; }
        public Type ParameterType { get; }
        public ParameterInfo ParameterInfo { get; }

        public IServiceProvider Services { get; }
        public object[] ExtraParameters { get; }
        public IReadOnlyDictionary<string, object> ValueMap { get; }

        public bool Handled { get; private set; }
        public object Value { get; private set; }

        internal MatchingParameterEventArgs(ParameterInfo parameterInfo, IServiceProvider services, object[] extraParameters, IReadOnlyDictionary<string, object> valueMap)
        {
            ParameterInfo = parameterInfo;
            ParameterName = parameterInfo.Name;
            ParameterType = parameterInfo.ParameterType;
            Services = services;
            ExtraParameters = extraParameters;
            ValueMap = valueMap;
        }

        public void SetValue(object value)
        {
            if (Handled)
                throw new Exception("cannot set value in mutiple times");
            Handled = true;
            Value = value;
        }
    }
}
