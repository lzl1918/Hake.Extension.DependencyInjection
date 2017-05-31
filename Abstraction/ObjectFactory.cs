using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public static class ObjectFactory
    {
        public static object CreateInstance(Type instanceType, params object[] extraParameters)
        {
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            TypeInfo instanceTypeInfo = instanceType.GetTypeInfo();

            CheckInstanceTypeOrThrow(instanceType);
            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                foreach (object extraParam in extraParameters)
                    if (instanceType.IsAssignableFrom(extraParam.GetType()))
                        return extraParam;
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors();
            foreach (ConstructorInfo constructor in constructors)
            {
                object[] constructParameterValues;
                if (TryMatchMethodParameters(constructor, extraParameters, out constructParameterValues))
                {
                    try
                    {
                        object instance = constructor.Invoke(constructParameterValues);
                        return instance;
                    }
                    catch (TargetInvocationException ex)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                        throw;
                    }
                }
            }
            throw new Exception("cannot find any constructor to initialize instance");
        }
        public static T CreateInstance<T>(params object[] extraParameters)
        {
            return (T)CreateInstance(typeof(T), extraParameters);
        }
        public static object CreateInstance(Type instanceType, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (valueMap == null)
                return CreateInstance(instanceType, extraParameters);

            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            CheckInstanceTypeOrThrow(instanceType);
            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                object value;
                if (valueMap.TryGetValue("value", out value) == true && instanceType.IsAssignableFrom(value.GetType()))
                    return value;

                foreach (object extraParam in extraParameters)
                    if (instanceType.IsAssignableFrom(extraParam.GetType()))
                        return extraParam;
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors();
            foreach (ConstructorInfo constructor in constructors)
            {
                object[] constructParameterValues;
                if (TryMatchMethodParameters(constructor, valueMap, extraParameters, out constructParameterValues))
                {
                    try
                    {
                        object instance = constructor.Invoke(constructParameterValues);
                        return instance;
                    }
                    catch (TargetInvocationException ex)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                        throw;
                    }
                }
            }
            throw new Exception("cannot find any constructor to initialize instance");
        }
        public static T CreateInstance<T>(IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            return (T)CreateInstance(typeof(T), valueMap, extraParameters);
        }
        public static object CreateInstance(IServiceProvider services, Type instanceType, params object[] extraParameters)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            CheckInstanceTypeOrThrow(instanceType);
            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                foreach (object extraParam in extraParameters)
                    if (instanceType.IsAssignableFrom(extraParam.GetType()))
                        return extraParam;
                object value;
                if (services.TryGetService(instanceType, out value) == true)
                    return value;
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors();
            foreach (ConstructorInfo constructor in constructors)
            {
                object[] constructParameterValues;
                if (TryMatchMethodParameters(constructor, services, extraParameters, out constructParameterValues))
                {
                    try
                    {
                        object instance = constructor.Invoke(constructParameterValues);
                        return instance;
                    }
                    catch (TargetInvocationException ex)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                        throw;
                    }
                }
            }
            throw new InvalidOperationException("cannot find any constructor to initialize instance");
        }
        public static T CreateInstance<T>(IServiceProvider services, params object[] extraParameters)
        {
            return (T)CreateInstance(services, typeof(T), extraParameters);
        }
        public static object CreateInstance(IServiceProvider services, Type instanceType, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (valueMap == null)
                return CreateInstance(services, instanceType, extraParameters);

            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            CheckInstanceTypeOrThrow(instanceType);
            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                object value;
                if (valueMap.TryGetValue("value", out value) == true && instanceType.IsAssignableFrom(value.GetType()))
                    return value;

                foreach (object extraParam in extraParameters)
                    if (instanceType.IsAssignableFrom(extraParam.GetType()))
                        return extraParam;

                if (services.TryGetService(instanceType, out value) == true)
                    return value;
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors();
            foreach (ConstructorInfo constructor in constructors)
            {
                object[] constructParameterValues;
                if (TryMatchMethodParameters(constructor, services, valueMap, extraParameters, out constructParameterValues))
                {
                    try
                    {
                        object instance = constructor.Invoke(constructParameterValues);
                        return instance;
                    }
                    catch (TargetInvocationException ex)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                        throw;
                    }
                }
            }
            throw new Exception("cannot find any constructor to initialize instance");
        }
        public static T CreateInstance<T>(IServiceProvider services, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            return (T)CreateInstance(services, typeof(T), valueMap, extraParameters);
        }

        private static void CheckInstanceTypeOrThrow(Type instanceType)
        {
            TypeInfo instanceTypeInfo = instanceType.GetTypeInfo();
            if (instanceTypeInfo.IsArray == true)
                throw new InvalidOperationException($"cannot create instance of array {instanceType.FullName}");
            if (instanceTypeInfo.IsEnum == true)
                throw new InvalidOperationException($"cannot create instance of enum type {instanceType.FullName}");
            if (instanceTypeInfo.IsClass == false)
            {
                if (instanceTypeInfo.IsValueType == false)
                    throw new InvalidOperationException($"cannot create instance of non-class or non-value type {instanceType.FullName}");
            }
            if (instanceTypeInfo.IsAbstract == true)
                throw new InvalidOperationException($"cannot create instance of abstract class {instanceType.FullName}");
            if (instanceTypeInfo.IsInterface == true)
                throw new InvalidOperationException($"cannot create instance of interface {instanceType.FullName}");
        }


        public static object InvokeMethod(object instance, string methodName, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            Type instanceType = instance.GetType();
            InvokeMethodResult result;
            foreach (MethodInfo method in instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != methodName)
                    continue;

                result = TryInvokeMethod(instance, method, extraParameters);
                if (result.IsExecuted == false)
                    continue;
                if (result.IsExecutionSucceeded == false)
                    throw result.Exception;
                if (result.HasReturnValueBySignature == false)
                    return null;
                else
                    return result.ReturnValue;
            }
            throw new InvalidOperationException($"cannot find any matched method {methodName} of instance {instance}");
        }
        public static object InvokeMethod(object instance, string methodName, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));
            if (valueMap == null)
                return InvokeMethod(instance, methodName, extraParameters);

            Type instanceType = instance.GetType();
            InvokeMethodResult result;
            foreach (MethodInfo method in instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != methodName)
                    continue;

                result = TryInvokeMethod(instance, method, valueMap, extraParameters);
                if (result.IsExecuted == false)
                    continue;
                if (result.IsExecutionSucceeded == false)
                    throw result.Exception;
                if (result.HasReturnValueBySignature == false)
                    return null;
                else
                    return result.ReturnValue;
            }
            throw new InvalidOperationException($"cannot find any matched method {methodName} of instance {instance}");
        }
        public static object InvokeMethod(object instance, string methodName, IServiceProvider services, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));
            if (services == null)
                return InvokeMethod(instance, methodName, extraParameters);

            Type instanceType = instance.GetType();
            InvokeMethodResult result;
            foreach (MethodInfo method in instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != methodName)
                    continue;

                result = TryInvokeMethod(instance, method, services, extraParameters);
                if (result.IsExecuted == false)
                    continue;
                if (result.IsExecutionSucceeded == false)
                    throw result.Exception;
                if (result.HasReturnValueBySignature == false)
                    return null;
                else
                    return result.ReturnValue;
            }
            throw new InvalidOperationException($"cannot find any matched method {methodName} of instance {instance}");
        }
        public static object InvokeMethod(object instance, string methodName, IServiceProvider services, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));
            if (valueMap == null)
                return InvokeMethod(instance, methodName, services, extraParameters);
            if (services == null)
                return InvokeMethod(instance, methodName, valueMap, extraParameters);

            Type instanceType = instance.GetType();
            InvokeMethodResult result;
            foreach (MethodInfo method in instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != methodName)
                    continue;

                result = TryInvokeMethod(instance, method, services, valueMap, extraParameters);
                if (result.IsExecuted == false)
                    continue;
                if (result.IsExecutionSucceeded == false)
                    throw result.Exception;
                if (result.HasReturnValueBySignature == false)
                    return null;
                else
                    return result.ReturnValue;
            }
            throw new InvalidOperationException($"cannot find any matched method {methodName} of instance {instance}");
        }
        public static T InvokeMethod<T>(object instance, string methodName, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, methodName, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, string methodName, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, methodName, valueMap, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, string methodName, IServiceProvider services, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, methodName, services, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, string methodName, IServiceProvider services, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, methodName, services, valueMap, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }

        public static object InvokeMethod(object instance, MethodInfo method, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            InvokeMethodResult result = TryInvokeMethod(instance, method, extraParameters);
            if (result.IsExecuted == false)
                throw new Exception("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, MethodInfo method, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (valueMap == null)
                return InvokeMethod(instance, method, extraParameters);

            InvokeMethodResult result = TryInvokeMethod(instance, method, valueMap, extraParameters);
            if (result.IsExecuted == false)
                throw new Exception("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, MethodInfo method, IServiceProvider services, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (services == null)
                return InvokeMethod(instance, method, extraParameters);

            InvokeMethodResult result = TryInvokeMethod(instance, method, services, extraParameters);
            if (result.IsExecuted == false)
                throw new Exception("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, MethodInfo method, IServiceProvider services, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (valueMap == null)
                return InvokeMethod(instance, method, services, extraParameters);
            if (services == null)
                return InvokeMethod(instance, method, valueMap, extraParameters);

            InvokeMethodResult result = TryInvokeMethod(instance, method, services, valueMap, extraParameters);
            if (result.IsExecuted == false)
                throw new Exception("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static T InvokeMethod<T>(object instance, MethodInfo method, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, method, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, MethodInfo method, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, method, valueMap, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, MethodInfo method, IServiceProvider services, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, method, services, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, MethodInfo method, IServiceProvider services, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            object result = InvokeMethod(instance, method, services, valueMap, extraParameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }

        private static InvokeMethodResult TryInvokeMethod(object instance, MethodInfo method, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            object[] parameterValues;
            bool parameterMatchResult;

            if (extraParameters == null)
                extraParameters = new object[0];

            parameterMatchResult = TryMatchMethodParameters(method, extraParameters, out parameterValues);
            if (parameterMatchResult == false)
                return new InvokeMethodResult(false, null, method.ReturnType, null);
            try
            {
                object returnValue = method.Invoke(instance, parameterValues);
                return new InvokeMethodResult(true, returnValue, method.ReturnType, null);
            }
            catch (TargetInvocationException ex)
            {
                return new InvokeMethodResult(true, null, method.ReturnType, ex.InnerException);
            }
            catch
            {
                throw;
            }
        }
        private static InvokeMethodResult TryInvokeMethod(object instance, MethodInfo method, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            object[] parameterValues;
            bool parameterMatchResult;

            if (extraParameters == null)
                extraParameters = new object[0];

            parameterMatchResult = TryMatchMethodParameters(method, valueMap, extraParameters, out parameterValues);
            if (parameterMatchResult == false)
                return new InvokeMethodResult(false, null, method.ReturnType, null);
            try
            {
                object returnValue = method.Invoke(instance, parameterValues);
                return new InvokeMethodResult(true, returnValue, method.ReturnType, null);
            }
            catch (TargetInvocationException ex)
            {
                return new InvokeMethodResult(true, null, method.ReturnType, ex.InnerException);
            }
            catch
            {
                throw;
            }
        }
        private static InvokeMethodResult TryInvokeMethod(object instance, MethodInfo method, IServiceProvider services, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            object[] parameterValues;
            bool parameterMatchResult;

            if (extraParameters == null)
                extraParameters = new object[0];

            parameterMatchResult = TryMatchMethodParameters(method, services, extraParameters, out parameterValues);
            if (parameterMatchResult == false)
                return new InvokeMethodResult(false, null, method.ReturnType, null);
            try
            {
                object returnValue = method.Invoke(instance, parameterValues);
                return new InvokeMethodResult(true, returnValue, method.ReturnType, null);
            }
            catch (TargetInvocationException ex)
            {
                return new InvokeMethodResult(true, null, method.ReturnType, ex.InnerException);
            }
            catch
            {
                throw;
            }
        }
        private static InvokeMethodResult TryInvokeMethod(object instance, MethodInfo method, IServiceProvider services, IReadOnlyDictionary<string, object> valueMap, params object[] extraParameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            object[] parameterValues;
            bool parameterMatchResult;

            if (extraParameters == null)
                extraParameters = new object[0];

            parameterMatchResult = TryMatchMethodParameters(method, services, valueMap, extraParameters, out parameterValues);
            if (parameterMatchResult == false)
                return new InvokeMethodResult(false, null, method.ReturnType, null);
            try
            {
                object returnValue = method.Invoke(instance, parameterValues);
                return new InvokeMethodResult(true, returnValue, method.ReturnType, null);
            }
            catch (TargetInvocationException ex)
            {
                return new InvokeMethodResult(true, null, method.ReturnType, ex.InnerException);
            }
            catch
            {
                throw;
            }
        }

        private static bool TryMatchMethodParameters(MethodBase method, object[] extraParameters, out object[] matchedParameters)
        {
            if (method == null)
            {
                matchedParameters = null;
                return false;
            }

            ParameterInfo[] parameterInfos = method.GetParameters();
            object[] matchedValues = new object[parameterInfos.Length];
            int extraParameterCount = extraParameters.Length;
            bool[] extraParameterUsed = new bool[extraParameterCount];
            bool parameterFullMatched = true;
            bool currentMatchResult;
            object currentMatchedValue;
            int matchExtraStart = 0;
            int matchExtraEnd = extraParameterCount - 1;
            int searchIndex;
            int currentParameterIndex = 0;
            ParamArrayAttribute paramsAttribute;
            Type paramsElementType;
            Type extraParamType;
            foreach (ParameterInfo parameter in parameterInfos)
            {
                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null && parameter.ParameterType.IsArray)
                {
                    List<object> passedParameters = new List<object>();
                    paramsElementType = parameter.ParameterType.GetElementType();
                    for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                    {
                        if (extraParameterUsed[searchIndex] == true)
                            continue;

                        if (paramsElementType.IsAssignableFrom(extraParameters[searchIndex].GetType()))
                        {
                            extraParameterUsed[searchIndex] = true;
                            if (searchIndex == matchExtraStart)
                                matchExtraStart++;
                            if (searchIndex == matchExtraEnd)
                                matchExtraEnd--;
                            passedParameters.Add(extraParameters[searchIndex]);
                        }
                    }
                    matchedValues[currentParameterIndex] = passedParameters.ToArray();
                    currentParameterIndex++;
                    continue;
                }

                currentMatchResult = false;
                currentMatchedValue = null;

                // match extra parameters
                for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                {
                    if (extraParameterUsed[searchIndex] == true)
                        continue;

                    extraParamType = extraParameters[searchIndex].GetType();
                    if (parameter.ParameterType.IsAssignableFrom(extraParamType) == true)
                    {
                        currentMatchResult = true;
                        currentMatchedValue = extraParameters[searchIndex];
                        extraParameterUsed[searchIndex] = true;
                        if (searchIndex == matchExtraStart)
                            matchExtraStart++;
                        if (searchIndex == matchExtraEnd)
                            matchExtraEnd--;
                        break;
                    }
                }

                // match default values
                if (currentMatchResult == false)
                {
                    if (parameter.HasDefaultValue)
                    {
                        currentMatchResult = true;
                        currentMatchedValue = parameter.DefaultValue;
                    }
                }

                // match failed
                if (currentMatchResult == false)
                {
                    parameterFullMatched = false;
                    break;
                }
                matchedValues[currentParameterIndex] = currentMatchedValue;
                currentParameterIndex++;
            }
            if (parameterFullMatched == true)
            {
                matchedParameters = matchedValues;
                return true;
            }
            else
            {
                matchedParameters = null;
                return false;
            }

        }
        private static bool TryMatchMethodParameters(MethodBase method, IReadOnlyDictionary<string, object> valueMap, object[] extraParameters, out object[] matchedParameters)
        {
            if (method == null)
            {
                matchedParameters = null;
                return false;
            }

            ParameterInfo[] parameterInfos = method.GetParameters();
            object[] matchedValues = new object[parameterInfos.Length];
            int extraParameterCount = extraParameters.Length;
            bool[] extraParameterUsed = new bool[extraParameterCount];
            bool parameterFullMatched = true;
            bool currentMatchResult;
            object currentMatchedValue;
            int matchExtraStart = 0;
            int matchExtraEnd = extraParameterCount - 1;
            int searchIndex;
            int currentParameterIndex = 0;
            ParamArrayAttribute paramsAttribute;
            Type paramsElementType;
            Type extraParamType;
            foreach (ParameterInfo parameter in parameterInfos)
            {
                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null && parameter.ParameterType.IsArray)
                {
                    currentMatchResult = TryMatchParameterFromDictionary(parameter, valueMap, out currentMatchedValue);
                    if (currentMatchResult == true)
                    {
                        matchedValues[currentParameterIndex] = currentMatchedValue;
                        currentParameterIndex++;
                        continue;
                    }

                    List<object> passedParameters = new List<object>();
                    paramsElementType = parameter.ParameterType.GetElementType();
                    for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                    {
                        if (extraParameterUsed[searchIndex] == true)
                            continue;

                        if (paramsElementType.IsAssignableFrom(extraParameters[searchIndex].GetType()))
                        {
                            extraParameterUsed[searchIndex] = true;
                            if (searchIndex == matchExtraStart)
                                matchExtraStart++;
                            if (searchIndex == matchExtraEnd)
                                matchExtraEnd--;
                            passedParameters.Add(extraParameters[searchIndex]);
                        }
                    }
                    matchedValues[currentParameterIndex] = passedParameters.ToArray();
                    currentParameterIndex++;
                    continue;
                }

                currentMatchResult = false;
                currentMatchedValue = null;

                // match value maps
                currentMatchResult = TryMatchParameterFromDictionary(parameter, valueMap, out currentMatchedValue);

                // match extra parameters
                if (currentMatchResult == false)
                {
                    for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                    {
                        if (extraParameterUsed[searchIndex] == true)
                            continue;
                        extraParamType = extraParameters[searchIndex].GetType();
                        if (parameter.ParameterType.IsAssignableFrom(extraParamType) == true)
                        {
                            currentMatchResult = true;
                            currentMatchedValue = extraParameters[searchIndex];
                            extraParameterUsed[searchIndex] = true;
                            if (searchIndex == matchExtraStart)
                                matchExtraStart++;
                            if (searchIndex == matchExtraEnd)
                                matchExtraEnd--;
                            break;
                        }
                    }
                }

                // match default values
                if (currentMatchResult == false)
                {
                    if (parameter.HasDefaultValue)
                    {
                        currentMatchResult = true;
                        currentMatchedValue = parameter.DefaultValue;
                    }
                }

                // match failed
                if (currentMatchResult == false)
                {
                    parameterFullMatched = false;
                    break;
                }
                matchedValues[currentParameterIndex] = currentMatchedValue;
                currentParameterIndex++;
            }

            if (parameterFullMatched == true)
            {
                matchedParameters = matchedValues;
                return true;
            }
            else
            {
                matchedParameters = null;
                return false;
            }
        }
        private static bool TryMatchMethodParameters(MethodBase method, IServiceProvider services, object[] extraParameters, out object[] matchedParameters)
        {
            if (services == null)
                return TryMatchMethodParameters(method, extraParameters, out matchedParameters);

            if (method == null)
            {
                matchedParameters = null;
                return false;
            }

            ParameterInfo[] parameterInfos = method.GetParameters();
            object[] matchedValues = new object[parameterInfos.Length];
            int extraParameterCount = extraParameters.Length;
            bool[] extraParameterUsed = new bool[extraParameterCount];
            bool parameterFullMatched = true;
            bool currentMatchResult;
            object currentMatchedValue;
            int matchExtraStart = 0;
            int matchExtraEnd = extraParameterCount - 1;
            int searchIndex;
            int currentParameterIndex = 0;
            ParamArrayAttribute paramsAttribute;
            Type paramsElementType;
            Type extraParamType;
            foreach (ParameterInfo parameter in parameterInfos)
            {
                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null && parameter.ParameterType.IsArray)
                {
                    List<object> passedParameters = new List<object>();
                    paramsElementType = parameter.ParameterType.GetElementType();
                    for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                    {
                        if (extraParameterUsed[searchIndex] == true)
                            continue;

                        if (paramsElementType.IsAssignableFrom(extraParameters[searchIndex].GetType()))
                        {
                            extraParameterUsed[searchIndex] = true;
                            if (searchIndex == matchExtraStart)
                                matchExtraStart++;
                            if (searchIndex == matchExtraEnd)
                                matchExtraEnd--;
                            passedParameters.Add(extraParameters[searchIndex]);
                        }
                    }
                    matchedValues[currentParameterIndex] = passedParameters.ToArray();
                    currentParameterIndex++;
                    continue;
                }

                currentMatchResult = false;
                currentMatchedValue = null;

                // match extra parameters
                if (extraParameters != null)
                {
                    for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                    {
                        if (extraParameterUsed[searchIndex] == true)
                            continue;
                        extraParamType = extraParameters[searchIndex].GetType();
                        if (parameter.ParameterType.IsAssignableFrom(extraParamType) == true)
                        {
                            currentMatchResult = true;
                            currentMatchedValue = extraParameters[searchIndex];
                            extraParameterUsed[searchIndex] = true;
                            if (searchIndex == matchExtraStart)
                                matchExtraStart++;
                            if (searchIndex == matchExtraEnd)
                                matchExtraEnd--;
                            break;
                        }
                    }
                }

                // match services
                if (currentMatchResult == false)
                    currentMatchResult = services.TryGetService(parameter.ParameterType, out currentMatchedValue);

                // match default values
                if (currentMatchResult == false)
                {
                    if (parameter.HasDefaultValue)
                    {
                        currentMatchResult = true;
                        currentMatchedValue = parameter.DefaultValue;
                    }
                }

                // match failed
                if (currentMatchResult == false)
                {
                    parameterFullMatched = false;
                    break;
                }
                matchedValues[currentParameterIndex] = currentMatchedValue;
                currentParameterIndex++;
            }
            if (parameterFullMatched == true)
            {
                matchedParameters = matchedValues;
                return true;
            }
            else
            {
                matchedParameters = null;
                return false;
            }
        }
        private static bool TryMatchMethodParameters(MethodBase method, IServiceProvider services, IReadOnlyDictionary<string, object> valueMap, object[] extraParameters, out object[] matchedParameters)
        {
            if (services == null)
                return TryMatchMethodParameters(method, valueMap, extraParameters, out matchedParameters);

            if (valueMap == null)
                return TryMatchMethodParameters(method, services, extraParameters, out matchedParameters);

            if (method == null)
            {
                matchedParameters = null;
                return false;
            }

            ParameterInfo[] parameterInfos = method.GetParameters();
            object[] matchedValues = new object[parameterInfos.Length];
            int extraParameterCount = extraParameters.Length;
            bool[] extraParameterUsed = new bool[extraParameterCount];
            bool parameterFullMatched = true;
            bool currentMatchResult;
            object currentMatchedValue;
            int matchExtraStart = 0;
            int matchExtraEnd = extraParameterCount - 1;
            int searchIndex;
            int currentParameterIndex = 0;
            ParamArrayAttribute paramsAttribute;
            Type paramsElementType;
            Type extraParamType;
            foreach (ParameterInfo parameter in parameterInfos)
            {
                paramsAttribute = parameter.GetCustomAttribute<ParamArrayAttribute>();
                if (paramsAttribute != null && parameter.ParameterType.IsArray)
                {
                    currentMatchResult = TryMatchParameterFromDictionary(parameter, valueMap, out currentMatchedValue);
                    if (currentMatchResult == true)
                    {
                        matchedValues[currentParameterIndex] = currentMatchedValue;
                        currentParameterIndex++;
                        continue;
                    }

                    List<object> passedParameters = new List<object>();
                    paramsElementType = parameter.ParameterType.GetElementType();
                    for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                    {
                        if (extraParameterUsed[searchIndex] == true)
                            continue;

                        if (paramsElementType.IsAssignableFrom(extraParameters[searchIndex].GetType()))
                        {
                            extraParameterUsed[searchIndex] = true;
                            if (searchIndex == matchExtraStart)
                                matchExtraStart++;
                            if (searchIndex == matchExtraEnd)
                                matchExtraEnd--;
                            passedParameters.Add(extraParameters[searchIndex]);
                        }
                    }
                    matchedValues[currentParameterIndex] = passedParameters.ToArray();
                    currentParameterIndex++;
                    continue;
                }

                currentMatchResult = false;
                currentMatchedValue = null;

                // match value maps
                currentMatchResult = TryMatchParameterFromDictionary(parameter, valueMap, out currentMatchedValue);

                // match extra parameters
                if (currentMatchResult == false)
                {
                    for (searchIndex = matchExtraStart; searchIndex <= matchExtraEnd; searchIndex++)
                    {
                        if (extraParameterUsed[searchIndex] == true)
                            continue;
                        extraParamType = extraParameters[searchIndex].GetType();
                        if (parameter.ParameterType.IsAssignableFrom(extraParamType) == true)
                        {
                            currentMatchResult = true;
                            currentMatchedValue = extraParameters[searchIndex];
                            extraParameterUsed[searchIndex] = true;
                            if (searchIndex == matchExtraStart)
                                matchExtraStart++;
                            if (searchIndex == matchExtraEnd)
                                matchExtraEnd--;
                            break;
                        }
                    }
                }

                // match services
                if (currentMatchResult == false)
                    currentMatchResult = services.TryGetService(parameter.ParameterType, out currentMatchedValue);

                // match default values
                if (currentMatchResult == false)
                {
                    if (parameter.HasDefaultValue)
                    {
                        currentMatchResult = true;
                        currentMatchedValue = parameter.DefaultValue;
                    }
                }

                // match failed
                if (currentMatchResult == false)
                {
                    parameterFullMatched = false;
                    break;
                }
                matchedValues[currentParameterIndex] = currentMatchedValue;
                currentParameterIndex++;
            }

            if (parameterFullMatched == true)
            {
                matchedParameters = matchedValues;
                return true;
            }
            else
            {
                matchedParameters = null;
                return false;
            }
        }

        private static bool TryMatchParameterFromDictionary(ParameterInfo parameterInfo, IReadOnlyDictionary<string, object> valueMap, out object matchedValue)
        {
            string paramName = parameterInfo.Name.ToLower();
            object mapedParam;
            if (valueMap.TryGetValue(paramName, out mapedParam) == true)
            {
                if (parameterInfo.ParameterType.IsAssignableFrom(mapedParam.GetType()) == true)
                {
                    matchedValue = mapedParam;
                    return true;
                }
                else
                {
                    try
                    {
                        matchedValue = Convert.ChangeType(mapedParam, parameterInfo.ParameterType);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // TODO: remove this line
                        throw ex;
                    }
                }
            }
            matchedValue = null;
            return false;
        }
    }
}
