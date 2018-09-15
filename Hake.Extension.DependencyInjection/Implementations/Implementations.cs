using Hake.Extension.DependencyInjection.Abstraction;
using Hake.Extension.DependencyInjection.Implementations.Internals;
using System;

namespace Hake.Extension.DependencyInjection
{
    public static class Implementation
    {
        public static IServiceCollection CreateServiceCollection()
        {
            return new ServiceCollection();
        }

        public static IServiceScopeFactory CreateServiceScopeFactory(IReadOnlyServiceCollection serviceCollection)
        {
            return new ServiceScopeFactory(serviceCollection);
        }
    }
}
