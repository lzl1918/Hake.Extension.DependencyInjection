using Hake.Extension.DependencyInjection.Abstraction;
using Hake.Extension.DependencyInjection.Utils;
using System;
using System.Collections.Generic;

namespace Hake.Extension.DependencyInjection.Implementations.Internals
{
    internal sealed class ScopedServiceProvider : IServiceProvider, IDisposable
    {
        private readonly static Type IServiceProviderType = typeof(IServiceProvider);

        private IReadOnlyServiceCollection serviceCollection;
        private TypedCache<object> instances;
        public ScopedServiceProvider(IReadOnlyServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            this.serviceCollection = serviceCollection;
            this.instances = new TypedCache<object>(capacity: 64);
        }

        public void Dispose()
        {
            if (instances == null)
                return;

            foreach (object instance in instances.GetItems())
            {
                if (instance is IDisposable disposable)
                    disposable.Dispose();
            }
            instances.Clear();
            instances = null;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                return null;
            if (IServiceProviderType.Equals(serviceType))
                return this;
            ServiceDescriptor serviceDescriptor = serviceCollection.GetDescriptor(serviceType);
            string typeName = serviceType.FullName;
            switch (serviceDescriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                case ServiceLifetime.Scoped:
                    instances.GetOrInsert(serviceType, out object instance, type => serviceDescriptor.CreateInstance(this));
                    return instance;

                case ServiceLifetime.Transient:
                    return serviceDescriptor.CreateInstance(this);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
