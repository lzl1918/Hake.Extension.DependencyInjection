using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Implementations
{
    public static class Implementation
    {
        public static IServiceCollection CreateServiceCollection()
        {
            return new InternalImplementations.ServiceCollection();
        }
        public static IServiceProvider CreateServiceProvider(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return new InternalImplementations.ServiceProvider(services);
        }
    }
}
