using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public static class ServiceCollectionExtension
    {
        public static ServiceDescriptor GetDescriptor<TService>(this IServiceCollection services)
        {
            return services.GetDescriptor(typeof(TService));
        }
        
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Transient<TService, TImplementation>());
            return services;
        }
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services) where TService : class
        {
            services.Add(ServiceDescriptor.Transient<TService>());
            return services;
        }
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Transient<TService, TImplementation>(implementationFactory));
            return services;
        }
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            services.Add(ServiceDescriptor.Transient<TService>(implementationFactory));
            return services;
        }
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            services.Add(ServiceDescriptor.Transient(serviceType, implementationType));
            return services;
        }
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType)
        {
            services.Add(ServiceDescriptor.Transient(serviceType));
            return services;
        }
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            services.Add(ServiceDescriptor.Transient(serviceType, implementationFactory));
            return services;
        }
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Scoped(typeof(TService), typeof(TImplementation)));
            return services;
        }
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services) where TService : class
        {
            services.Add(ServiceDescriptor.Scoped(typeof(TService), typeof(TService)));
            return services;
        }
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Scoped(typeof(TService), implementationFactory));
            return services;

        }
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            services.Add(ServiceDescriptor.Scoped(typeof(TService), implementationFactory));
            return services;
        }
        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            services.Add(ServiceDescriptor.Scoped(serviceType, implementationType));
            return services;
        }
        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType)
        {
            services.Add(ServiceDescriptor.Scoped(serviceType));
            return services;
        }
        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            services.Add(ServiceDescriptor.Scoped(serviceType, implementationFactory));
            return services;
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Singleton(typeof(TService), typeof(TImplementation)));
            return services;
        }
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services) where TService : class
        {
            services.Add(ServiceDescriptor.Singleton(typeof(TService)));
            return services;
        }
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
        {
            services.Add(ServiceDescriptor.Singleton(typeof(TService), implementationFactory));
            return services;
        }
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            services.Add(ServiceDescriptor.Singleton(typeof(TService), implementationFactory));
            return services;
        }
        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            services.Add(ServiceDescriptor.Singleton(serviceType, implementationType));
            return services;
        }
        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType)
        {
            services.Add(ServiceDescriptor.Singleton(serviceType));
            return services;
        }
        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            services.Add(ServiceDescriptor.Singleton(serviceType, implementationFactory));
            return services;
        }
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance) where TService : class
        {
            services.Add(ServiceDescriptor.Singleton(typeof(TService), implementationInstance));
            return services;
        }
        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, object implementationInstance)
        {
            services.Add(ServiceDescriptor.Singleton(serviceType, implementationInstance));
            return services;
        }
    }
}
