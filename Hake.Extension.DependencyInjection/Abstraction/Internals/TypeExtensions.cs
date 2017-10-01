using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Hake.Extension.DependencyInjection.Utils;
using System.Linq;

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
        private const int CACHE_SIZE = 20;
        private static TypedCache<TypeInfo> typeInfoCache = new TypedCache<TypeInfo>(CACHE_SIZE);
        private static Func<Type, TypeInfo> typeInfoInsertion = t => t.GetTypeInfo();

        private static TypedCache<Type> ienumerableCache = new TypedCache<Type>(CACHE_SIZE);
        private static Func<Type, Type> ienumerableInsertion = t =>
        {
#if NETSTANDARD2_0 || NET452
            if (t.IsInterface && t.GetNameWithNamespace() == "System.Collections.Generic.IEnumerable`1")
                return t;
            if (t.IsInterface && t.GetNameWithNamespace() == "System.Collections.IEnumerable")
                return t;

            Type result = t.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (result != null)
                return result;
            return t.GetInterface("System.Collections.IEnumerable");
#elif NETSTANDARD1_2
            TypeInfo typeInfo = t.GetTypeInfoFromCache();
            if (typeInfo.IsInterface && t.GetNameWithNamespace() == "System.Collections.Generic.IEnumerable`1")
                return t;
            if (typeInfo.IsInterface && t.GetNameWithNamespace() == "System.Collections.IEnumerable")
                return t;

            Type result = typeInfo.ImplementedInterfaces.FirstOrDefault(i => i.GetNameWithNamespace() == "System.Collections.Generic.IEnumerable`1");
            if (result != null)
                return result;
            return typeInfo.ImplementedInterfaces.FirstOrDefault(i => i.GetNameWithNamespace() == "System.Collections.IEnumerable");
#else
            // raise compile error
            abcdefg
#endif
        };


        private static TypedCache<object> defaultValueCache = new TypedCache<object>(CACHE_SIZE);
        private static Func<Type, object> defaultValueInsertion = t =>
        {
#if NETSTANDARD2_0 || NET452
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            if (t.IsArray)
                return Activator.CreateInstance(t, 0);
            Type elementType;
            if (t.IsAssignableFromList(out elementType))
                return CreateEmptyList(elementType);
            return null;
#elif NETSTANDARD1_2
            TypeInfo typeInfo = t.GetTypeInfoFromCache();
            if (typeInfo.IsValueType)
                return Activator.CreateInstance(t);
            if (typeInfo.IsArray)
                return Activator.CreateInstance(t, 0);
            Type elementType;
            if (t.IsAssignableFromList(out elementType))
                return CreateEmptyList(elementType);
            return null;
#else
            // raise compile error
            abcdefg
#endif
        };

        private static TypedCache<Type> elementTypeCache = new TypedCache<Type>(CACHE_SIZE);
        private static Func<Type, Type> elementTypeInsertion = t =>
        {
#if NETSTANDARD2_0 || NET452
            if (t.GetNameWithNamespace() == "System.Collections.Generic.IEnumerable`1")
                return t.GetGenericArguments()[0];
            else if (t.GetNameWithNamespace() == "System.Collections.IEnumerable")
                return TypeList.OBJECT_TYPE;
            else
                return null;
#elif NETSTANDARD1_2
            if (t.GetNameWithNamespace() == "System.Collections.Generic.IEnumerable`1")
                return t.GenericTypeArguments[0];
            else if (t.GetNameWithNamespace() == "System.Collections.IEnumerable")
                return TypeList.OBJECT_TYPE;
            else
                return null;
#else
            // raise compile error
            abcdefg
#endif
        };

        private static TypedCache<ConstructorInfo[]> constructorCache = new TypedCache<ConstructorInfo[]>(CACHE_SIZE);
        private static Func<Type, ConstructorInfo[]> constructorInsertion = t =>
        {
#if NETSTANDARD2_0 || NET452
            return t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
#elif NETSTANDARD1_2
            TypeInfo typeInfo = t.GetTypeInfoFromCache();
            return typeInfo.DeclaredConstructors.Where(m => m.IsPublic).ToArray();
#else
            // raise compile error
            abcdefg
#endif
        };

        private static TypedCache<bool> isConvertibleCache = new TypedCache<bool>(CACHE_SIZE);
        private static Func<Type, bool> isConvertibleInsertion = t =>
        {
#if NETSTANDARD2_0 || NET452
            return t.GetInterface("System.IConvertible") != null;
#elif NETSTANDARD1_2
            // always false, because System.IConvertible does not exists in netstandard1.2
            TypeInfo typeInfo = t.GetTypeInfoFromCache();
            return typeInfo.ImplementedInterfaces.FirstOrDefault(i => i.GetNameWithNamespace() == "System.IConvertible") != null;
#else
            // raise compile error
            abcdefg
#endif
        };

        private static TypedCache<bool> isIDisposableCache = new TypedCache<bool>(CACHE_SIZE);
        private static Func<Type, bool> isIDisposableInsertion = t =>
        {
#if NETSTANDARD2_0 || NET452
            return t.GetInterface("System.IDisposable") != null;
#elif NETSTANDARD1_2
            TypeInfo typeInfo = t.GetTypeInfoFromCache();
            return typeInfo.ImplementedInterfaces.FirstOrDefault(i => i.GetNameWithNamespace() == "System.IDisposable") != null;
#else
            // raise compile error
            abcdefg
#endif
        };

        public static MethodInfo[] GetMethodOverloads(this Type type, string methodName, bool declaredOnly)
        {
#if NETSTANDARD2_0 || NET452
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            if(declaredOnly)
                flags |= BindingFlags.DeclaredOnly;
            return type.GetMember(methodName, MemberTypes.Method, flags).Select(m => (MethodInfo)m).ToArray();
#elif NETSTANDARD1_2
            if (!declaredOnly)
                return type.GetRuntimeMethods().Where(m => m.Name == methodName && m.Attributes.HasFlag(MethodAttributes.Public) && !m.Attributes.HasFlag(MethodAttributes.Static)).ToArray();
            else
            {
                TypeInfo typeInfo = type.GetTypeInfoFromCache();
                return typeInfo.GetDeclaredMethods(methodName).Where(m => m.Attributes.HasFlag(MethodAttributes.Public) && !m.Attributes.HasFlag(MethodAttributes.Static)).ToArray();
            }
#else
            // raise compile error
            abcdefg
#endif
        }

        public static bool IsConvertible(this Type type)
        {
            bool result;
            isConvertibleCache.GetOrInsert(type, out result, isConvertibleInsertion);
            return result;
        }

        public static bool IsDisposable(this Type type)
        {
            bool result;
            isIDisposableCache.GetOrInsert(type, out result, isIDisposableInsertion);
            return result;
        }

        public static ConstructorInfo[] GetConstructorsFromCache(this Type type)
        {
            ConstructorInfo[] constructors;
            constructorCache.GetOrInsert(type, out constructors, constructorInsertion);
            return constructors;
        }


        public static object DefaultValue(this Type type)
        {
            object value;
            defaultValueCache.GetOrInsert(type, out value, defaultValueInsertion);
            return value;
        }

        public static bool IsAssignableFromList(this Type type, out Type elementType)
        {
            Type ienumType = type.GetIEnumerableInterface();
            if (ienumType == null)
            {
                elementType = null;
                return false;
            }
            Type itemType = GetElementType(ienumType);
            elementType = itemType;
            Type listType = TypeList.LIST_TYPE.MakeGenericType(itemType);
#if NETSTANDARD2_0 || NET452
            if (type.IsAssignableFrom(listType))
                return true;
#elif NETSTANDARD1_2
            TypeInfo typeInfo = type.GetTypeInfoFromCache();
            TypeInfo listTypeInfo = listType.GetTypeInfoFromCache();
            if (typeInfo.IsAssignableFrom(listTypeInfo))
                return true;
#else
            // raise compile error
            abcdefg
#endif
            return false;
        }
        public static object CreateEmptyList(Type elementType)
        {
            Type listType = TypeList.LIST_TYPE.MakeGenericType(elementType);
            return Activator.CreateInstance(listType);
        }
        public static bool IsIEnumerable(this Type type)
        {
            Type enumerableType;
            ienumerableCache.GetOrInsert(type, out enumerableType, ienumerableInsertion);
            return enumerableType != null;
        }
        public static Type GetIEnumerableInterface(this Type type)
        {
            Type enumerableType;
            ienumerableCache.GetOrInsert(type, out enumerableType, ienumerableInsertion);
            return enumerableType;
        }
        public static Type GetElementType(Type type)
        {
            Type elementType;
            elementTypeCache.GetOrInsert(type, out elementType, elementTypeInsertion);
            return elementType;
        }

        public static TypeInfo GetTypeInfoFromCache(this Type type)
        {
            TypeInfo typeInfo;
            typeInfoCache.GetOrInsert(type, out typeInfo, typeInfoInsertion);
            return typeInfo;
        }

        //        public static string GetFullName(this Type type)
        //        {
        //#if NETSTANDARD2_0 || NET452
        //            if(!type.IsGenericType)
        //                return $"{type.Namespace}.{type.Name}";
        //#elif NETSTANDARD1_2
        //            TypeInfo typeInfo = type.GetTypeInfoFromCache();
        //            if(!typeInfo.IsGenericType)
        //                return $"{type.Namespace}.{type.Name}";
        //            type.GenericTypeArguments
        //#else
        //            // raise compile error
        //            abcdefg
        //#endif
        //            return $"{type.Namespace}.{type.Name}";
        //        }

        public static string GetNameWithNamespace(this Type type) => $"{type.Namespace}.{type.Name}";
    }
}
