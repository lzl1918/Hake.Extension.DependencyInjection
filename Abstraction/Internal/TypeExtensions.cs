using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction.Internal
{
    internal static class TypeExtensions
    {
        private static readonly Type LIST_TYPE = typeof(List<object>).GetGenericTypeDefinition();

        public static object DefaultValue(this TypeInfo type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type.AsType());

            if (type.IsArray)
                return Activator.CreateInstance(type.AsType(), 0);
            Type elementType;
            if (type.IsAssignableFromList(out elementType))
            {
                return CreateList(elementType);
            }
            return null;
        }

        public static bool IsAssignableFromList(this TypeInfo type, out Type elementType)
        {
            Type enumType = type.GetIEnumerableInfo();
            if (enumType == null)
            {
                elementType = null;
                return false;
            }
            Type itemType = GetListElementType(enumType);
            elementType = itemType;
            Type listType = LIST_TYPE.MakeGenericType(itemType);
            if (type.IsAssignableFrom(listType))
                return true;
            return false;
        }
        public static object CreateList(Type elementType)
        {
            Type listType = LIST_TYPE.MakeGenericType(elementType);
            return Activator.CreateInstance(listType);
        }
        public static bool IsIEnumerable(this TypeInfo type)
        {
            return type.GetInterface("System.Collections.Generic.IEnumerable`1") != null;
        }
        public static Type GetIEnumerableInfo(this TypeInfo type)
        {
            if (type.IsInterface && type.Name == "IEnumerable`1" && type.Namespace == "System.Collections.Generic")
                return type.AsType();
            return type.GetInterface("System.Collections.Generic.IEnumerable`1");
        }
        public static Type GetListElementType(Type type)
        {
            return type.GetGenericArguments()[0];
        }
    }
}
