using Hake.Extension.DependencyInjection.Abstraction;
using System;

namespace Hake.Extension.DependencyInjection
{
    public static class Implementation
    {
        public static IServiceCollection CreateServiceCollection()
        {
            return new Implementations.Internals.ServiceCollection();
        }
        public static IServiceProvider CreateProvider(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return new Implementations.Internals.ServiceProvider(services);
        }
    }
}
