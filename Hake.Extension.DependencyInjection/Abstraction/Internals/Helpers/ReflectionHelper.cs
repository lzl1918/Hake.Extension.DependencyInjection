using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction.Internals.Helpers
{
    internal static class ReflectionHelper
    {
        public static IEnumerator GetEnumerator(object value)
        {
            Type valueType = value.GetType();
            Type ienumType = valueType.GetIEnumerable();
            if (ienumType == null)
                return null;
            MethodInfo getEnumeratorMethod = ienumType.GetMethod("GetEnumerator");
            return getEnumeratorMethod.Invoke(value, null) as IEnumerator;
        }

        public static object CreateArray(List<object> values, Type elementType)
        {
            int count = values.Count;
            Type arrayType = elementType.MakeArrayType();
            object array = Activator.CreateInstance(arrayType, count);
            MethodInfo method = arrayType.GetMethod("Set", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            object[] param = new object[2];
            for (int i = 0; i < count; i++)
            {
                param[0] = i;
                param[1] = values[i];
                method.Invoke(array, param);
            }
            return array;
        }
        public static object CreateList(List<object> values, Type elementType)
        {
            int count = values.Count;
            object list = TypeExtensions.CreateEmptyList(elementType);
            Type listType = list.GetType();
            MethodInfo method = listType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            object[] param = new object[1];
            for (int i = 0; i < count; i++)
            {
                param[0] = values[i];
                method.Invoke(list, param);
            }
            return list;
        }

        public static bool TryMatchValue(Type targetType, Type type, object value, out object target)
        {
            if (targetType.IsAssignableFrom(type))
            {
                target = value;
                return true;
            }
            else if (targetType.GetInterface("System.IConvertible") != null && type.GetInterface("System.IConvertible") != null)
            {
                try
                {
                    target = Convert.ChangeType(value, targetType);
                    return true;
                }
                catch
                {
                    target = null;
                    return false;
                }
            }

            ValueMatchingEventArgs args = ObjectFactory.RaiseValueMatchingEvent(targetType, type, value);
            if (args != null && args.Handled)
            {
                target = args.Value;
                return true;
            }
            target = null;
            return false;
        }
        public static bool TryFindValue(string name, Type targetType, IReadOnlyDictionary<string, object> options, out object result)
        {
            object valueFromOptions;
            if (!options.TryGetValue(name, out valueFromOptions))
            {
                result = null;
                return false;
            }

            if (valueFromOptions == null)
            {
                result = Activator.CreateInstance(targetType);
                return true;
            }
            else
            {
                Type valueType = valueFromOptions.GetType();
                if (TryMatchValue(targetType, valueType, valueFromOptions, out result))
                    return true;
                else
                    return TryMatchValueAsList(valueFromOptions, targetType, true, out result);
            }
        }
        public static bool TryMatchValueAsList(object value, Type targetType, bool allowEmpty, out object result)
        {
            Type elementType;
            List<object> values;
            if (targetType.IsArray)
            {
                elementType = targetType.GetElementType();
                values = ToList(value, elementType);
                if (allowEmpty || values.Count > 0)
                {
                    result = CreateArray(values, elementType);
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else if (targetType.IsAssignableFromList(out elementType))
            {
                values = ToList(value, elementType);
                if (allowEmpty || values.Count > 0)
                {
                    result = CreateList(values, elementType);
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = null;
            return false;
        }
        public static List<object> ToList(object value, Type elementType)
        {
            List<object> result = new List<object>();
            object objectResult;
            object current;
            IEnumerator enumerator;
            Type valueType = value.GetType();
            if (TryMatchValue(elementType, valueType, value, out objectResult))
                result.Add(objectResult);
            else if ((enumerator = GetEnumerator(value)) != null)
            {
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    if (TryMatchValue(elementType, current.GetType(), current, out objectResult))
                        result.Add(objectResult);
                }
            }
            return result;
        }
    }
}
