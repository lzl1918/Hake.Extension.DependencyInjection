using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction.Internal
{
    internal static class TypeExtensions
    {
        public static object DefaultValue(this TypeInfo type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type.AsType());
            return null;
        }
    }
}
