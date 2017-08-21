using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction.Internals
{
    internal static class TypeList
    {
        public static Type OBJECT_TYPE { get; } = typeof(object);
        public static Type LIST_TYPE { get; } = typeof(List<object>).GetGenericTypeDefinition();
        public static Type STRING_TYPE { get; } = typeof(string);
        public static Type VOID_TYPE { get; } = typeof(void);
    }

    internal static class TypeExtensions
    {
        public static object DefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            if (type.IsArray)
                return Activator.CreateInstance(type, 0);
            Type elementType;
            if (type.IsAssignableFromList(out elementType))
                return CreateEmptyList(elementType);
            return null;
        }

        public static bool IsAssignableFromList(this Type type, out Type elementType)
        {
            Type ienumType = type.GetIEnumerable();
            if (ienumType == null)
            {
                elementType = null;
                return false;
            }
            Type itemType = GetElementType(ienumType);
            elementType = itemType;
            Type listType = TypeList.LIST_TYPE.MakeGenericType(itemType);
            if (type.IsAssignableFrom(listType))
                return true;
            return false;
        }
        public static object CreateEmptyList(Type elementType)
        {
            Type listType = TypeList.LIST_TYPE.MakeGenericType(elementType);
            return Activator.CreateInstance(listType);
        }
        public static bool IsIEnumerable(this Type type)
        {
            return (
                type.GetInterface("System.Collections.Generic.IEnumerable`1") != null ||
                type.GetInterface("System.Collections.IEnumerable") != null);
        }
        public static Type GetIEnumerable(this Type type)
        {
            if (type.IsInterface && type.Name == "IEnumerable`1" && type.Namespace == "System.Collections.Generic")
                return type;
            if (type.IsInterface && type.Name == "IEnumerable" && type.Namespace == "System.Collections")
                return type;

            Type result = type.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (result != null)
                return result;
            return type.GetInterface("System.Collections.IEnumerable");
        }
        public static Type GetElementType(Type type)
        {
            if (type.Name == "IEnumerable`1")
                return type.GetGenericArguments()[0];
            else if (type.Name == "IEnumerable")
                return TypeList.OBJECT_TYPE;
            else
                return null;
        }
    }
}
