using Hake.Extension.DependencyInjection.Abstraction;
using System;

namespace Hake.Extension.DependencyInjection.Implementations.Internals
{
    internal sealed class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IReadOnlyServiceCollection serviceCollection;

        public ServiceScopeFactory(IReadOnlyServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));
            this.serviceCollection = serviceCollection;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(new ScopedServiceProviderFactory(), serviceCollection);
        }
    }
}
