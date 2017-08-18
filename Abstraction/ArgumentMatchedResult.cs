﻿using Hake.Extension.DependencyInjection.Abstraction.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    // parameter score:
    //      val_matched  : 100%
    //      val_default  : 75%
    //      type_default : 50%
    //      not_type_def : 25%
    //      not_matched  : 0%, fail
    internal sealed class ArgumentMatchedResult
    {
        private const double SINGLE_PARAMETER_SCORE = 10;
        private const double DEFAULT_VALUE_RATIO = 0.75;
        private const double TYPE_DEFAULT_RATIO = 0.5;
        private const double NOT_TYPE_DEFAULT_RATIO = 0.25;


        public double Score { get; }
        public bool IsPassed { get; }
        public MethodBase Method { get; }
        public object[] Result { get; }

        private ArgumentMatchedResult(MethodBase method, object[] result, double score, bool isPassed)
        {
            Method = method;
            Score = score;
            IsPassed = isPassed;
            Result = result;
        }

        private static object GetTypedArray(List<object> values, Type elementType)
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
        private static object GetTypedList(List<object> values, Type elementType)
        {
            int count = values.Count;
            object list = Internal.TypeExtensions.CreateList(elementType);
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

        private static bool TryMatchParameterType(TypeInfo assignLeft, TypeInfo assignRight, object input, out object value)
        {
            if (assignLeft.IsAssignableFrom(assignRight))
            {
                value = input;
                return true;
            }
            else if (assignLeft.GetInterface("System.IConvertible") != null && assignRight.GetInterface("System.IConvertible") != null)
            {
                try
                {
                    value = Convert.ChangeType(input, assignLeft.AsType());
                    return true;
                }
                catch
                {
                    value = null;
                    return false;
                }
            }

            ValueMatchingEventArgs args = ObjectFactory.RaiseValueMatchingEvent(assignLeft, assignRight, input);
            if (args != null && args.Handled)
            {
                value = args.Value;
                return true;
            }
            value = null;
            return false;
        }
        private static bool TryFindParameterFromDictionary(string paramName, TypeInfo paramTypeInfo, IReadOnlyDictionary<string, object> options, out object matchedValue)
        {
            object mappedParam;
            object value;
            if (options.TryGetValue(paramName, out mappedParam) && mappedParam != null)
            {
                Type valueType = mappedParam.GetType();
                TypeInfo valueTypeInfo = valueType.GetTypeInfo();
                if (TryMatchParameterType(paramTypeInfo, valueTypeInfo, mappedParam, out value))
                {
                    matchedValue = value;
                    return true;
                }
                else
                    return TryMatchValueToList(mappedParam, paramTypeInfo, true, out matchedValue);
            }
            else
            {
                matchedValue = null;
                return false;
            }
        }
        private static List<object> TryMatchParameterList(object input, TypeInfo elementTypeInfo)
        {
            TypeInfo inputTypeInfo = input.GetType().GetTypeInfo();
            Type enumType = inputTypeInfo.GetIEnumerableInfo();
            List<object> result = new List<object>();
            object value;
            if (TryMatchParameterType(elementTypeInfo, inputTypeInfo, input, out value))
                result.Add(value);
            else if (enumType != null)
            {
                MethodInfo getEnumeratorMethod = getEnumeratorMethod = enumType.GetMethod("GetEnumerator");
                object enumerator = getEnumeratorMethod.Invoke(input, null);
                Type enumeratorType = enumerator.GetType();
                MethodInfo currentMethod = enumeratorType.GetProperty("Current").GetMethod;
                MethodInfo moveNextMethod = enumeratorType.GetMethod("MoveNext");
                bool next;
                object current;
                while (true)
                {
                    next = (bool)moveNextMethod.Invoke(enumerator, null);
                    if (!next)
                        break;
                    current = currentMethod.Invoke(enumerator, null);
                    if (current == null)
                        continue;
                    if (TryMatchParameterType(elementTypeInfo, current.GetType().GetTypeInfo(), current, out value))
                        result.Add(value);
                }
            }
            return result;
        }
        private static bool TryMatchValueToList(object input, TypeInfo parameterType, bool allowEmpty, out object value)
        {
            Type elementType;
            TypeInfo elementTypeInfo;
            List<object> values;
            if (parameterType.IsArray)
            {
                elementType = parameterType.GetElementType();
                elementTypeInfo = elementType.GetTypeInfo();
                values = TryMatchParameterList(input, elementTypeInfo);
                if (allowEmpty || values.Count > 0)
                {
                    value = GetTypedArray(values, elementType);
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            // values -> list
            if (parameterType.IsAssignableFromList(out elementType))
            {
                elementTypeInfo = elementType.GetTypeInfo();
                values = TryMatchParameterList(input, elementTypeInfo);
                if (allowEmpty || values.Count > 0)
                {
                    value = GetTypedList(values, elementType);
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            value = null;
            return false;
        }
        private static double CalculateScore(int matchedCount, int defaultValueCount, int typeDefaultCount, int notTypeDefaultCount, int totalCount)
        {
            if (totalCount == 0)
                return SINGLE_PARAMETER_SCORE;

            double score = 0;
            score += matchedCount * SINGLE_PARAMETER_SCORE;
            score += defaultValueCount * SINGLE_PARAMETER_SCORE * DEFAULT_VALUE_RATIO;
            score += typeDefaultCount * SINGLE_PARAMETER_SCORE * TYPE_DEFAULT_RATIO;
            score += notTypeDefaultCount * SINGLE_PARAMETER_SCORE * NOT_TYPE_DEFAULT_RATIO;
            score /= totalCount;
            return score;
        }

        public static ArgumentMatchedResult Match(MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    result[paramIndex] = new object[0];
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }

                // no need of checking 'ref'

                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, null, null, null);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    result[paramIndex] = matchingEventArgs.Value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);
        }
        public static ArgumentMatchedResult Match(MethodBase method, object[] parameters)
        {
            if (parameters == null)
                return Match(method);

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ArgumentTraverseContext traverseContext = new ArgumentTraverseContext(parameters);
            bool traverseResult;
            int inputParameterCount = parameters.Length;
            TypeInfo[] inputParameterTypes = new TypeInfo[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType().GetTypeInfo();

            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    TypeInfo elementTypeInfo = elementType.GetTypeInfo();
                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (TryMatchParameterType(elementTypeInfo, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = GetTypedArray(extras, elementType);
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }
                // no need of checking 'ref'

                isValueFound = false;
                value = null;
                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchParameterType(paramTypeInfo, inputParameterTypes[index], input, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                traverseContext.Reset();
                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, null, null, traverseContext);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    value = matchingEventArgs.Value;
                    if (value != null)
                    {
                        traverseContext.Reset();
                        do
                        {
                            traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReferenceEquals(value, input)));
                            if (isValueFound)
                                break;
                        } while (traverseResult);
                    }
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchValueToList(input, paramTypeInfo, false, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);
        }
        public static ArgumentMatchedResult Match(MethodBase method, IReadOnlyDictionary<string, object> options)
        {
            if (options == null)
                return Match(method);

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            SortedSet<string> usedKeys = new SortedSet<string>();
            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    TypeInfo elementTypeInfo = elementType.GetTypeInfo();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (TryMatchParameterType(elementTypeInfo, pair.Value.GetType().GetTypeInfo(), pair.Value, out value))
                            extras.Add(value);
                    }
                    result[paramIndex] = GetTypedArray(extras, elementType);
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }
                // no need of checking 'ref'

                value = null;
                paramName = parameter.Name.ToLower();
                if (TryFindParameterFromDictionary(paramName, paramTypeInfo, options, out value))
                {
                    usedKeys.Add(paramName);
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, null, options, null);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    result[paramIndex] = matchingEventArgs.Value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);

        }
        public static ArgumentMatchedResult Match(MethodBase method, IReadOnlyDictionary<string, object> options, object[] parameters)
        {
            if (options == null)
                return Match(method, parameters);
            if (parameters == null)
                return Match(method, options);
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ArgumentTraverseContext traverseContext = new ArgumentTraverseContext(parameters);
            bool traverseResult;
            int inputParameterCount = parameters.Length;
            TypeInfo[] inputParameterTypes = new TypeInfo[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType().GetTypeInfo();

            SortedSet<string> usedKeys = new SortedSet<string>();
            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    TypeInfo elementTypeInfo = elementType.GetTypeInfo();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (TryMatchParameterType(elementTypeInfo, pair.Value.GetType().GetTypeInfo(), pair.Value, out value))
                            extras.Add(value);
                    }

                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (TryMatchParameterType(elementTypeInfo, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = GetTypedArray(extras, elementType);
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }
                // no need of checking 'ref'

                value = null;
                paramName = parameter.Name.ToLower();
                if (TryFindParameterFromDictionary(paramName, paramTypeInfo, options, out value))
                {
                    isValueFound = true;
                    usedKeys.Add(paramName);
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                isValueFound = false;
                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchParameterType(paramTypeInfo, inputParameterTypes[index], input, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                traverseContext.Reset();
                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, null, options, traverseContext);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    value = matchingEventArgs.Value;
                    if (value != null)
                    {
                        traverseContext.Reset();
                        do
                        {
                            traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReferenceEquals(value, input)));
                            if (isValueFound)
                                break;
                        } while (traverseResult);
                    }
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchValueToList(input, paramTypeInfo, false, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);

        }

        public static ArgumentMatchedResult Match(MethodBase method, IServiceProvider services)
        {
            if (services == null)
                return Match(method);
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    result[paramIndex] = new object[0];
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }

                // no need of checking 'ref'

                if (services.TryGetService(paramType, out value))
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, services, null, null);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    result[paramIndex] = matchingEventArgs.Value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);
        }
        public static ArgumentMatchedResult Match(MethodBase method, IServiceProvider services, object[] parameters)
        {
            if (services == null)
                return Match(method, parameters);
            if (parameters == null)
                return Match(method, services);
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ArgumentTraverseContext traverseContext = new ArgumentTraverseContext(parameters);
            bool traverseResult;
            int inputParameterCount = parameters.Length;
            TypeInfo[] inputParameterTypes = new TypeInfo[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType().GetTypeInfo();

            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    TypeInfo elementTypeInfo = elementType.GetTypeInfo();
                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (TryMatchParameterType(elementTypeInfo, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = GetTypedArray(extras, elementType);
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }
                // no need of checking 'ref'

                isValueFound = false;
                value = null;
                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchParameterType(paramTypeInfo, inputParameterTypes[index], input, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                if (services.TryGetService(paramType, out value))
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                traverseContext.Reset();
                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, services, null, traverseContext);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    value = matchingEventArgs.Value;
                    if (value != null)
                    {
                        traverseContext.Reset();
                        do
                        {
                            traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReferenceEquals(value, input)));
                            if (isValueFound)
                                break;
                        } while (traverseResult);
                    }
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchValueToList(input, paramTypeInfo, false, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);
        }
        public static ArgumentMatchedResult Match(MethodBase method, IServiceProvider services, IReadOnlyDictionary<string, object> options)
        {
            if (services == null)
                return Match(method, options);
            if (options == null)
                return Match(method, services);
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            SortedSet<string> usedKeys = new SortedSet<string>();
            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    TypeInfo elementTypeInfo = elementType.GetTypeInfo();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (TryMatchParameterType(elementTypeInfo, pair.Value.GetType().GetTypeInfo(), pair.Value, out value))
                            extras.Add(value);
                    }
                    result[paramIndex] = GetTypedArray(extras, elementType);
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }
                // no need of checking 'ref'

                value = null;
                paramName = parameter.Name.ToLower();
                if (TryFindParameterFromDictionary(paramName, paramTypeInfo, options, out value))
                {
                    usedKeys.Add(paramName);
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                if (services.TryGetService(paramType, out value))
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, services, options, null);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    result[paramIndex] = matchingEventArgs.Value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);

        }
        public static ArgumentMatchedResult Match(MethodBase method, IServiceProvider services, IReadOnlyDictionary<string, object> options, object[] parameters)
        {
            if (services == null)
                return Match(method, options, parameters);
            if (options == null)
                return Match(method, services, parameters);
            if (parameters == null)
                return Match(method, services, options);
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ArgumentTraverseContext traverseContext = new ArgumentTraverseContext(parameters);
            bool traverseResult;
            int inputParameterCount = parameters.Length;
            TypeInfo[] inputParameterTypes = new TypeInfo[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType().GetTypeInfo();

            SortedSet<string> usedKeys = new SortedSet<string>();
            ParameterInfo[] methodParameters = method.GetParameters();
            int paramCount = methodParameters.Length;
            object[] result = new object[paramCount];
            int paramIndex = 0;
            int matchedCount = 0;
            int defaultValueCount = 0;
            int typeDefaultCount = 0;
            int notTypeDefaultCount = 0;
            int notMatchedCount = 0;
            Type paramType;
            TypeInfo paramTypeInfo;
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramTypeInfo = paramType.GetTypeInfo();
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    TypeInfo elementTypeInfo = elementType.GetTypeInfo();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (TryMatchParameterType(elementTypeInfo, pair.Value.GetType().GetTypeInfo(), pair.Value, out value))
                            extras.Add(value);
                    }
                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (TryMatchParameterType(elementTypeInfo, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = GetTypedArray(extras, elementType);
                    matchedCount++;
                    continue;
                }

                if (parameter.IsOut)
                {
                    result[paramIndex] = null;
                    matchedCount++;
                    continue;
                }
                // no need of checking 'ref'

                value = null;
                paramName = parameter.Name.ToLower();
                if (TryFindParameterFromDictionary(paramName, paramTypeInfo, options, out value))
                {
                    isValueFound = true;
                    usedKeys.Add(paramName);
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                isValueFound = false;
                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchParameterType(paramTypeInfo, inputParameterTypes[index], input, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                if (services.TryGetService(paramType, out value))
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                traverseContext.Reset();
                matchingEventArgs = ObjectFactory.RaiseParameterMatchingEvent(parameter, services, options, traverseContext);
                if (matchingEventArgs != null && matchingEventArgs.Handled)
                {
                    value = matchingEventArgs.Value;
                    if (value != null)
                    {
                        traverseContext.Reset();
                        do
                        {
                            traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReferenceEquals(value, input)));
                            if (isValueFound)
                                break;
                        } while (traverseResult);
                    }
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    result[paramIndex] = parameter.DefaultValue;
                    defaultValueCount++;
                    continue;
                }

                traverseContext.Reset();
                do
                {
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = TryMatchValueToList(input, paramTypeInfo, false, out value)));
                    if (isValueFound)
                        break;
                } while (traverseResult);
                if (isValueFound)
                {
                    result[paramIndex] = value;
                    matchedCount++;
                    continue;
                }

                {
                    result[paramIndex] = paramTypeInfo.DefaultValue();
                    typeDefaultCount++;
                }
            }

            bool isPassed = notMatchedCount <= 0;
            double score = 0;
            if (isPassed)
                score = CalculateScore(matchedCount, defaultValueCount, typeDefaultCount, notTypeDefaultCount, paramCount);
            return new ArgumentMatchedResult(method, result, score, isPassed);

        }
    }
}
