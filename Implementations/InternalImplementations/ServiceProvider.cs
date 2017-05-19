using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Implementations.InternalImplementations
{
    internal sealed class ServiceProvider : IServiceProvider
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
    }
}
