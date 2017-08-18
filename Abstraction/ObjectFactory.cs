using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public static class ObjectFactory
    {
        public static event EventHandler<ParameterMatchingEventArgs> ParameterMatching;
        public static event EventHandler<ValueMatchingEventArgs> ValueMatching;

        internal static ValueMatchingEventArgs RaiseValueMatchingEvent(TypeInfo targetType, TypeInfo inputType, object inputValue)
        {
            if (ValueMatching == null)
                return null;

            ValueMatchingEventArgs args = new ValueMatchingEventArgs(targetType, inputType, inputValue);
            ValueMatching.Invoke(null, args);
            return args;
        }
        internal static ParameterMatchingEventArgs RaiseParameterMatchingEvent(ParameterInfo parameter, IServiceProvider services, IReadOnlyDictionary<string, object> options, ArgumentTraverseContext arguments)
        {
            if (ParameterMatching == null)
                return null;

            ParameterMatchingEventArgs args = new ParameterMatchingEventArgs(parameter, services, options, arguments);
            ParameterMatching.Invoke(null, args);
            return args;
        }

        public static object CreateInstance(Type instanceType, params object[] parameters)
        {
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            CheckInstanceTypeOrThrow(instanceType);
            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                object value;
                if (parameters.Length > 0 && TryFindBestMatchOfPrimitive(instanceType, parameters, out value))
                    return value;
                throw new InvalidOperationException($"primitive type {instanceType.Name} has no constructor");
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length <= 0)
                throw new InvalidOperationException($"cannot find any constructor to initialize instance of type {instanceType.Name}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m, parameters));
            else
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any constructor of type {instanceType.Name} that matchs given parameters");

            ConstructorInfo constructor = matchResult.Method as ConstructorInfo;
            try
            {
                object instance = constructor.Invoke(matchResult.Result);
                return instance;
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
        public static T CreateInstance<T>(params object[] parameters)
        {
            return (T)CreateInstance(typeof(T), parameters);
        }
        public static object CreateInstance(Type instanceType, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (namedParameters == null)
                return CreateInstance(instanceType, parameters);
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            CheckInstanceTypeOrThrow(instanceType);
            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                object value;
                if (TryFindValueOptionOfPrimitive(instanceType, namedParameters, out value))
                    return value;
                if (parameters.Length > 0 && TryFindBestMatchOfPrimitive(instanceType, parameters, out value))
                    return value;
                if (TryFindBestMatchOfPrimitive(instanceType, namedParameters, out value))
                    return value;
                throw new InvalidOperationException($"primitive type {instanceType.Name} has no constructor");
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length <= 0)
                throw new InvalidOperationException($"cannot find any constructor to initialize instance of type {instanceType.Name}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m, namedParameters, parameters));
            else
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m, namedParameters));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any constructor of type {instanceType.Name} that matchs given parameters");

            ConstructorInfo constructor = matchResult.Method as ConstructorInfo;
            try
            {
                object instance = constructor.Invoke(matchResult.Result);
                return instance;
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
        public static T CreateInstance<T>(IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            return (T)CreateInstance(typeof(T), namedParameters, parameters);
        }
        public static object CreateInstance(Type instanceType, IServiceProvider services, params object[] parameters)
        {
            if (services == null)
                return CreateInstance(instanceType, parameters);
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            CheckInstanceTypeOrThrow(instanceType);

            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                object value;
                if (parameters.Length > 0 && TryFindBestMatchOfPrimitive(instanceType, parameters, out value))
                    return value;
                throw new InvalidOperationException($"primitive type {instanceType.Name} has no constructor");
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length <= 0)
                throw new InvalidOperationException($"cannot find any constructor to initialize instance of type {instanceType.Name}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m, services, parameters));
            else
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m, services));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any constructor of type {instanceType.Name} that matchs given parameters");

            ConstructorInfo constructor = matchResult.Method as ConstructorInfo;
            try
            {
                object instance = constructor.Invoke(matchResult.Result);
                return instance;
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
        public static T CreateInstance<T>(IServiceProvider services, params object[] parameters)
        {
            return (T)CreateInstance(typeof(T), services, parameters);
        }
        public static object CreateInstance(Type instanceType, IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (namedParameters == null)
                return CreateInstance(instanceType, services, parameters);
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            CheckInstanceTypeOrThrow(instanceType);
            if (instanceType.GetTypeInfo().IsPrimitive)
            {
                object value;
                if (TryFindValueOptionOfPrimitive(instanceType, namedParameters, out value))
                    return value;
                if (parameters.Length > 0 && TryFindBestMatchOfPrimitive(instanceType, parameters, out value))
                    return value;
                if (TryFindBestMatchOfPrimitive(instanceType, namedParameters, out value))
                    return value;
                throw new InvalidOperationException($"primitive type {instanceType.Name} has no constructor");
            }

            ConstructorInfo[] constructors = instanceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length <= 0)
                throw new InvalidOperationException($"cannot find any constructor to initialize instance of type {instanceType.Name}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m, services, namedParameters, parameters));
            else
                matchResults = constructors.Select(m => ArgumentMatchedResult.Match(m, services, namedParameters));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any constructor of type {instanceType.Name} that matchs given parameters");

            ConstructorInfo constructor = matchResult.Method as ConstructorInfo;
            try
            {
                object instance = constructor.Invoke(matchResult.Result);
                return instance;
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
        public static T CreateInstance<T>(IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            return (T)CreateInstance(typeof(T), services, namedParameters, parameters);
        }

        private static bool TryFindBestMatchOfPrimitive(Type type, object[] parameters, out object value)
        {
            int paramCount = parameters.Length;
            TypeInfo[] paramTypes = new TypeInfo[paramCount];
            TypeInfo currentTypeInfo;
            Type currentType;
            object paramValue;
            for (int i = 0; i < paramCount; i++)
            {
                paramValue = parameters[i];
                currentType = paramValue.GetType();
                currentTypeInfo = currentType.GetTypeInfo();

                if (currentType.Equals(type))
                {
                    value = paramValue;
                    return true;
                }

                paramTypes[i] = currentTypeInfo;
            }
            for (int i = 0; i < paramCount; i++)
            {
                if (paramTypes[i].GetInterface("IConvertible") != null)
                {
                    try
                    {
                        value = Convert.ChangeType(parameters[i], type);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }
            value = null;
            return false;
        }
        private static bool TryFindBestMatchOfPrimitive(Type type, IReadOnlyDictionary<string, object> parameters, out object value)
        {
            TypeInfo currentTypeInfo;
            Type currentType;
            object paramValue;
            int paramCount = parameters.Count;
            TypeInfo[] paramTypes = new TypeInfo[paramCount];
            object[] paramValues = new object[paramCount];
            int i = 0;
            foreach (var pair in parameters)
            {
                paramValue = pair.Value;
                currentType = paramValue.GetType();
                currentTypeInfo = currentType.GetTypeInfo();
                if (currentType.Equals(type))
                {
                    value = paramValue;
                    return true;
                }

                paramTypes[i] = currentTypeInfo;
                paramValues[i] = paramValue;
                i++;
            }
            for (i = 0; i < paramCount; i++)
            {
                if (paramTypes[i].GetInterface("IConvertible") != null)
                {
                    try
                    {
                        value = Convert.ChangeType(paramValues[i], type);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }
            value = null;
            return false;
        }
        private static bool TryFindValueOptionOfPrimitive(Type type, IReadOnlyDictionary<string, object> parameters, out object value)
        {
            TypeInfo currentTypeInfo;
            Type currentType;
            object paramValue;
            if (parameters.TryGetValue("value", out paramValue))
            {
                currentType = paramValue.GetType();
                currentTypeInfo = currentType.GetTypeInfo();
                if (currentType.Equals(type))
                {
                    value = paramValue;
                    return true;
                }
                else if (currentTypeInfo.GetInterface("IConvertible") != null)
                {
                    try
                    {
                        value = Convert.ChangeType(paramValue, type);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }
            value = null;
            return false;
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


        public static object InvokeMethod(object instance, string methodName, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            Type instanceType = instance.GetType();
            MethodInfo[] methods = instanceType.GetMember(methodName, BindingFlags.Public | BindingFlags.Instance).Where(m => m is MethodInfo).Select(m => m as MethodInfo).ToArray();
            if (methods.Length <= 0)
                throw new InvalidOperationException("cannot find method {methodName} of instance {instance}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m, parameters));
            else
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any method {methods[0].Name} of instance {instance} that matchs given parameters");
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, string methodName, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));
            if (namedParameters == null)
                return InvokeMethod(instance, methodName, parameters);

            Type instanceType = instance.GetType();
            MethodInfo[] methods = instanceType.GetMember(methodName, BindingFlags.Public | BindingFlags.Instance).Where(m => m is MethodInfo).Select(m => m as MethodInfo).ToArray();
            if (methods.Length <= 0)
                throw new InvalidOperationException("cannot find method {methodName} of instance {instance}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m, namedParameters, parameters));
            else
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m, namedParameters));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any method {methods[0].Name} of instance {instance} that matchs given parameters");
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, string methodName, IServiceProvider services, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));
            if (services == null)
                return InvokeMethod(instance, methodName, parameters);

            Type instanceType = instance.GetType();
            MethodInfo[] methods = instanceType.GetMember(methodName, BindingFlags.Public | BindingFlags.Instance).Where(m => m is MethodInfo).Select(m => m as MethodInfo).ToArray();
            if (methods.Length <= 0)
                throw new InvalidOperationException("cannot find method {methodName} of instance {instance}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m, services, parameters));
            else
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m, services));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any method {methods[0].Name} of instance {instance} that matchs given parameters");
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, string methodName, IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));
            if (namedParameters == null)
                return InvokeMethod(instance, methodName, services, parameters);
            if (services == null)
                return InvokeMethod(instance, methodName, namedParameters, parameters);

            Type instanceType = instance.GetType();
            MethodInfo[] methods = instanceType.GetMember(methodName, BindingFlags.Public | BindingFlags.Instance).Where(m => m is MethodInfo).Select(m => m as MethodInfo).ToArray();
            if (methods.Length <= 0)
                throw new InvalidOperationException("cannot find method {methodName} of instance {instance}");

            IEnumerable<ArgumentMatchedResult> matchResults = null;
            if (parameters.Length > 0)
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m, services, namedParameters, parameters));
            else
                matchResults = methods.Select(m => ArgumentMatchedResult.Match(m, services, namedParameters));

            ArgumentMatchedResult matchResult = FindBestMatch(matchResults);
            if (matchResult == null)
                throw new InvalidOperationException($"cannot find any method {methods[0].Name} of instance {instance} that matchs given parameters");
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }

        public static T InvokeMethod<T>(object instance, string methodName, params object[] parameters)
        {
            object result = InvokeMethod(instance, methodName, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, string methodName, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            object result = InvokeMethod(instance, methodName, namedParameters, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, string methodName, IServiceProvider services, params object[] parameters)
        {
            object result = InvokeMethod(instance, methodName, services, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, string methodName, IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            object result = InvokeMethod(instance, methodName, services, namedParameters, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }

        public static object InvokeMethod(object instance, MethodInfo method, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method);
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecuted == false)
                throw new InvalidOperationException("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, MethodInfo method, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (namedParameters == null)
                return InvokeMethod(instance, method, parameters);

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, namedParameters, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method, namedParameters);
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecuted == false)
                throw new InvalidOperationException("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, MethodInfo method, IServiceProvider services, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (services == null)
                return InvokeMethod(instance, method, parameters);

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, services, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method, services);
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecuted == false)
                throw new InvalidOperationException("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }
        public static object InvokeMethod(object instance, MethodInfo method, IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (namedParameters == null)
                return InvokeMethod(instance, method, services, parameters);
            if (services == null)
                return InvokeMethod(instance, method, namedParameters, parameters);

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, services, namedParameters, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method, services, namedParameters);
            InvokeMethodResult result = TryInvokeMethod(instance, matchResult);
            if (result.IsExecuted == false)
                throw new InvalidOperationException("cannot invoke method due to insufficient parameters");
            if (result.IsExecutionSucceeded == false)
                throw result.Exception;
            if (result.HasReturnValueBySignature == false)
                return null;
            else
                return result.ReturnValue;
        }

        public static T InvokeMethod<T>(object instance, MethodInfo method, params object[] parameters)
        {
            object result = InvokeMethod(instance, method, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, MethodInfo method, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            object result = InvokeMethod(instance, method, namedParameters, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, MethodInfo method, IServiceProvider services, params object[] parameters)
        {
            object result = InvokeMethod(instance, method, services, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }
        public static T InvokeMethod<T>(object instance, MethodInfo method, IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            object result = InvokeMethod(instance, method, services, namedParameters, parameters);
            if (result == null)
                return default(T);
            else
                return (T)result;
        }

        public static MethodInvokeContext CreateInvokeContext(MethodInfo method, params object[] parameters)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method);

            if (!matchResult.IsPassed)
                return null;
            return new MethodInvokeContext(matchResult.Score, matchResult.Result, method);
        }
        public static MethodInvokeContext CreateInvokeContext(MethodInfo method, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (namedParameters == null)
                return CreateInvokeContext(method, parameters);

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, namedParameters, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method, namedParameters);

            if (!matchResult.IsPassed)
                return null;
            return new MethodInvokeContext(matchResult.Score, matchResult.Result, method);
        }
        public static MethodInvokeContext CreateInvokeContext(MethodInfo method, IServiceProvider services, params object[] parameters)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (services == null)
                return CreateInvokeContext(method, parameters);

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, services, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method, services);

            if (!matchResult.IsPassed)
                return null;
            return new MethodInvokeContext(matchResult.Score, matchResult.Result, method);
        }
        public static MethodInvokeContext CreateInvokeContext(MethodInfo method, IServiceProvider services, IReadOnlyDictionary<string, object> namedParameters, params object[] parameters)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (namedParameters == null)
                return CreateInvokeContext(method, services, parameters);
            if (services == null)
                return CreateInvokeContext(method, namedParameters, parameters);

            ArgumentMatchedResult matchResult = null;
            if (parameters.Length > 0)
                matchResult = ArgumentMatchedResult.Match(method, services, namedParameters, parameters);
            else
                matchResult = ArgumentMatchedResult.Match(method, services, namedParameters);

            if (!matchResult.IsPassed)
                return null;
            return new MethodInvokeContext(matchResult.Score, matchResult.Result, method);
        }

        private static InvokeMethodResult TryInvokeMethod(object instance, ArgumentMatchedResult matchResult)
        {
            MethodInfo method = matchResult.Method as MethodInfo;
            if (matchResult.IsPassed)
            {
                try
                {
                    object returnValue = method.Invoke(instance, matchResult.Result);
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
            else
            {
                return new InvokeMethodResult(false, null, method.ReturnType, null);
            }
        }
        private static ArgumentMatchedResult FindBestMatch(IEnumerable<ArgumentMatchedResult> results)
        {
            double maxScore = 0;
            ArgumentMatchedResult matchResult = null;
            foreach (ArgumentMatchedResult m in results)
            {
                if (!m.IsPassed)
                    continue;
                if (m.Score > maxScore)
                {
                    matchResult = m;
                    maxScore = m.Score;
                }
            }
            return matchResult;

        }
    }
}
