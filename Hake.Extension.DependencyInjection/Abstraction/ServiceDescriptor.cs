﻿using Hake.Extension.DependencyInjection.Abstraction.Internals;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class ServiceDescriptor
    {
        public ServiceLifetime Lifetime { get; }
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public object ImplementationInstance { get; }
        public Func<IServiceProvider, object> ImplementationFactory { get; }

        private ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime) : this(serviceType, lifetime)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            ImplementationType = implementationType;
        }
        private ServiceDescriptor(Type serviceType, object instance) : this(serviceType, ServiceLifetime.Singleton)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            ImplementationInstance = instance;
        }
        private ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : this(serviceType, lifetime)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            ImplementationFactory = factory;
        }
        private ServiceDescriptor(Type serviceType, ServiceLifetime lifetime)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            ServiceType = serviceType;
            Lifetime = lifetime;
        }

        public static ServiceDescriptor Transient<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            return Transient(typeof(TService), typeof(TImplementation));
        }
        public static ServiceDescriptor Transient<TService>() where TService : class
        {
            return Transient(typeof(TService), typeof(TService));
        }
        public static ServiceDescriptor Transient<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
        {
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Transient);
        }
        public static ServiceDescriptor Transient<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Transient);
        }
        public static ServiceDescriptor Transient(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            return new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        }
        public static ServiceDescriptor Transient(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return new ServiceDescriptor(serviceType, serviceType, ServiceLifetime.Transient);
        }
        public static ServiceDescriptor Transient(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(serviceType, implementationFactory, ServiceLifetime.Transient);
        }

        public static ServiceDescriptor Scoped<TService, TImplementation>() where TService : class where TImplementation : TService
        {
            return Scoped(typeof(TService), typeof(TImplementation));
        }
        public static ServiceDescriptor Scoped<TService>() where TService : class
        {
            return Scoped(typeof(TService), typeof(TService));
        }
        public static ServiceDescriptor Scoped<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
        {
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
        }
        public static ServiceDescriptor Scoped<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
        }
        public static ServiceDescriptor Scoped(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            return new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Scoped);
        }
        public static ServiceDescriptor Scoped(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return new ServiceDescriptor(serviceType, serviceType, ServiceLifetime.Scoped);
        }
        public static ServiceDescriptor Scoped(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(serviceType, implementationFactory, ServiceLifetime.Scoped);
        }

        public static ServiceDescriptor Singleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            return Singleton(typeof(TService), typeof(TImplementation));
        }
        public static ServiceDescriptor Singleton<TService>() where TService : class
        {
            return Singleton(typeof(TService), typeof(TService));
        }
        public static ServiceDescriptor Singleton<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
        {
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
        }
        public static ServiceDescriptor Singleton<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
        }
        public static ServiceDescriptor Singleton(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            return new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton);
        }
        public static ServiceDescriptor Singleton(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return new ServiceDescriptor(serviceType, serviceType, ServiceLifetime.Singleton);
        }
        public static ServiceDescriptor Singleton(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return new ServiceDescriptor(serviceType, implementationFactory, ServiceLifetime.Singleton);
        }
        public static ServiceDescriptor Singleton<TService>(TService implementationInstance) where TService : class
        {
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return Singleton(typeof(TService), implementationInstance);
        }
        public static ServiceDescriptor Singleton(Type serviceType, object implementationInstance)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return new ServiceDescriptor(serviceType, implementationInstance);
        }

    }
}
