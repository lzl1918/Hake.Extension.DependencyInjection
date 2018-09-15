using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;

namespace Hake.Extension.DependencyInjection.Implementations.Internals
{
    internal sealed class ServiceScope : IServiceScope
    {
        private IServiceProvider serviceProvider;
        private readonly IReadOnlyServiceCollection serviceCollection;
        private readonly IServiceProviderFactory serviceProviderFactory;

        public IServiceProvider ServiceProvider => serviceProvider ?? (serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection));

        public ServiceScope(IServiceProviderFactory serviceProviderFactory, IReadOnlyServiceCollection serviceCollection)
        {
            this.serviceProviderFactory = serviceProviderFactory;
            this.serviceCollection = serviceCollection;
        }

        public void Dispose()
        {
            if (serviceProvider == null)
                return;
            if (serviceProvider is IDisposable disposable)
                disposable.Dispose();
            serviceProvider = null;
        }
    }
}
