using System;
using System.Collections.Generic;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public static class ServiceProviderExtension
    {
        public static T GetService<T>(this IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            return (T)provider.GetService(typeof(T));
        }

        public static bool TryGetService(this IServiceProvider provider, Type serviceType, out object instance)
        {
            if (provider == null || serviceType == null)
            {
                instance = null;
                return false;
            }

            if (provider is IExtendedServiceProvider extendedProvider)
                return extendedProvider.TryGetService(serviceType, out instance);

            try
            {
                instance = provider.GetService(serviceType);
                return true;
            }
            catch (Exception)
            {
                instance = null;
                return false;
            }
        }

        public static bool TryGetService<T>(this IServiceProvider provider, out T instance)
        {
            if (provider == null)
            {
                instance = default(T);
                return false;
            }

            object temperoryInstance;
            if (TryGetService(provider, typeof(T), out temperoryInstance))
            {
                instance = (T)temperoryInstance;
                return true;
            }
            else
            {
                instance = default(T);
                return false;
            }
        }

        public static object CreateInstance(this IServiceProvider provider, Type instanceType, params object[] parameters)
        {
            return ObjectFactory.CreateInstance(instanceType, provider, parameters);
        }
        public static object CreateInstance(this IServiceProvider provider, Type instanceType, IReadOnlyDictionary<string, object> options, params object[] parameters)
        {
            return ObjectFactory.CreateInstance(instanceType, provider, options, parameters);
        }
        public static T CreateInstance<T>(this IServiceProvider provider, params object[] parameters)
        {
            return ObjectFactory.CreateInstance<T>(provider, parameters);
        }
        public static T CreateInstance<T>(this IServiceProvider provider, IReadOnlyDictionary<string, object> options, params object[] parameters)
        {
            return ObjectFactory.CreateInstance<T>(provider, options, parameters);
        }

        public static bool TryCreateInstance(this IServiceProvider provider, Type instanceType, out object instance, params object[] parameters)
        {
            try
            {
                instance = CreateInstance(provider, instanceType, parameters);
                return true;
            }
            catch
            {
                instance = null;
                return false;
            }
        }
        public static bool TryCreateInstance(this IServiceProvider provider, Type instanceType, IReadOnlyDictionary<string, object> options, out object instance, params object[] parameters)
        {
            try
            {
                instance = CreateInstance(provider, instanceType, options, parameters);
                return true;
            }
            catch
            {
                instance = null;
                return false;
            }
        }
        public static bool TryCreateInstance<T>(this IServiceProvider provider, out T instance, params object[] parameters)
        {
            try
            {
                instance = CreateInstance<T>(provider, parameters);
                return true;
            }
            catch
            {
                instance = default(T);
                return false;
            }
        }
        public static bool TryCreateInstance<T>(this IServiceProvider provider, IReadOnlyDictionary<string, object> options, out T instance, params object[] parameters)
        {
            try
            {
                instance = CreateInstance<T>(provider, options, parameters);
                return true;
            }
            catch
            {
                instance = default(T);
                return false;
            }
        }
    }
}
