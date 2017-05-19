using System;
using System.Collections.Generic;
using System.Text;

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
            if (serviceType == null || provider == null)
            {
                instance = null;
                return false;
            }
            try
            {
                instance = provider.GetService(serviceType);
                return true;
            }
            catch
            {
                instance = null;
                return false;
            }
        }

        public static bool TryGetService<T>(this IServiceProvider provider, out T instance)
        {
            object temperoryInstance;
            bool result = TryGetService(provider, typeof(T), out temperoryInstance);
            if (result == true)
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
            return ObjectFactory.CreateInstance(provider, instanceType, parameters);
        }
        public static object CreateInstance(this IServiceProvider provider, Type instanceType, IReadOnlyDictionary<string, object> valueMap, params object[] parameters)
        {
            return ObjectFactory.CreateInstance(provider, instanceType, valueMap, parameters);
        }
        public static T CreateInstance<T>(this IServiceProvider provider, params object[] parameters)
        {
            return ObjectFactory.CreateInstance<T>(provider, parameters);
        }
        public static T CreateInstance<T>(this IServiceProvider provider, IReadOnlyDictionary<string, object> valueMap, params object[] parameters)
        {
            return ObjectFactory.CreateInstance<T>(provider, valueMap, parameters);
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
        public static bool TryCreateInstance(this IServiceProvider provider, Type instanceType, IReadOnlyDictionary<string, object> valueMap, out object instance, params object[] parameters)
        {
            try
            {
                instance = CreateInstance(provider, instanceType, valueMap, parameters);
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
        public static bool TryCreateInstance<T>(this IServiceProvider provider, IReadOnlyDictionary<string, object> valueMap, out T instance, params object[] parameters)
        {
            try
            {
                instance = CreateInstance<T>(provider, valueMap, parameters);
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
