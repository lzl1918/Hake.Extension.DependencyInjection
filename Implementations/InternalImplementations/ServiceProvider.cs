using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Implementations.InternalImplementations
{
    internal sealed class ServiceProvider : IExtendedServiceProvider
    {
        private IServiceCollection services;
        public ServiceProvider(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            this.services = services;
        }
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                return null;

            return services.GetDescriptor(serviceType).GetInstance(this);
        }

        public bool TryGetService(Type serviceType, out object instance)
        {
            if (serviceType == null)
            {
                instance = null;
                return false;
            }
            ServiceDescriptor descriptor;
            if (services.TryGetDescriptor(serviceType, out descriptor) == false)
            {
                instance = null;
                return false;
            }
            instance = descriptor.GetInstance(this);
            return true;
        }
    }
}
