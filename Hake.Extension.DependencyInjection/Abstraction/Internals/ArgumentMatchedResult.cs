using Hake.Extension.DependencyInjection.Abstraction.Internals.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction.Internals
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
            Type[] inputParameterTypes = new Type[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType();

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
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (ReflectionHelper.TryMatchValue(elementType, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = ReflectionHelper.CreateArray(extras, elementType);
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValue(paramType, inputParameterTypes[index], input, out value)));
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValueAsList(input, paramType, false, out value)));
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
                    result[paramIndex] = paramType.DefaultValue();
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
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (ReflectionHelper.TryMatchValue(elementType, pair.Value.GetType(), pair.Value, out value))
                            extras.Add(value);
                    }
                    result[paramIndex] = ReflectionHelper.CreateArray(extras, elementType);
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
                if (ReflectionHelper.TryFindValue(paramName, paramType, options, out value))
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
                    result[paramIndex] = paramType.DefaultValue();
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
            Type[] inputParameterTypes = new Type[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType();

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
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (ReflectionHelper.TryMatchValue(elementType, pair.Value.GetType(), pair.Value, out value))
                            extras.Add(value);
                    }

                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (ReflectionHelper.TryMatchValue(elementType, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = ReflectionHelper.CreateArray(extras, elementType);
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
                if (ReflectionHelper.TryFindValue(paramName, paramType, options, out value))
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValue(paramType, inputParameterTypes[index], input, out value)));
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValueAsList(input, paramType, false, out value)));
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
                    result[paramIndex] = paramType.DefaultValue();
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
            Type[] inputParameterTypes = new TypeInfo[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType();

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
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (ReflectionHelper.TryMatchValue(elementType, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = ReflectionHelper.CreateArray(extras, elementType);
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValue(paramType, inputParameterTypes[index], input, out value)));
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValueAsList(input, paramType, false, out value)));
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
                    result[paramIndex] = paramType.DefaultValue();
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
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (ReflectionHelper.TryMatchValue(elementType, pair.Value.GetType(), pair.Value, out value))
                            extras.Add(value);
                    }
                    result[paramIndex] = ReflectionHelper.CreateArray(extras, elementType);
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
                if (ReflectionHelper.TryFindValue(paramName, paramType, options, out value))
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
                    result[paramIndex] = paramType.DefaultValue();
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
            Type[] inputParameterTypes = new Type[inputParameterCount];
            for (int i = 0; i < inputParameterCount; i++)
                inputParameterTypes[i] = parameters[i].GetType();

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
            ParamArrayAttribute paramsAttribute;
            ParameterMatchingEventArgs matchingEventArgs;
            object value;
            bool isValueFound;
            string paramName;
            foreach (ParameterInfo parameter in methodParameters)
            {
                paramType = parameter.ParameterType;
                paramIndex = parameter.Position;

                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null)
                {
                    List<object> extras = new List<object>();
                    Type elementType = paramType.GetElementType();
                    foreach (var pair in options)
                    {
                        if (usedKeys.Contains(pair.Key))
                            continue;
                        if (ReflectionHelper.TryMatchValue(elementType, pair.Value.GetType(), pair.Value, out value))
                            extras.Add(value);
                    }
                    traverseContext.Reset();
                    do
                    {
                        traverseResult = traverseContext.GoNext((input, index) =>
                        {
                            if (ReflectionHelper.TryMatchValue(elementType, inputParameterTypes[index], input, out value))
                            {
                                extras.Add(value);
                                return true;
                            }
                            return false;
                        });
                    } while (traverseResult);
                    result[paramIndex] = ReflectionHelper.CreateArray(extras, elementType);
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
                if (ReflectionHelper.TryFindValue(paramName, paramType, options, out value))
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValue(paramType, inputParameterTypes[index], input, out value)));
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
                    traverseResult = traverseContext.GoNext((input, index) => (isValueFound = ReflectionHelper.TryMatchValueAsList(input, paramType, false, out value)));
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
                    result[paramIndex] = paramType.DefaultValue();
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
