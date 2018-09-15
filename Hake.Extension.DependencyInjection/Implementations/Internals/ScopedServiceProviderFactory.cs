using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Implementations.Internals
{
    internal sealed class ScopedServiceProviderFactory : IServiceProviderFactory
    {
        public IServiceProvider CreateServiceProvider(IReadOnlyServiceCollection serviceCollection)
        {
            return new ScopedServiceProvider(serviceCollection);
        }
    }
}
