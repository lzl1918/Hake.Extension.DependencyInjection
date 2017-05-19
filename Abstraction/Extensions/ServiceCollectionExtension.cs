using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public static class ServiceCollectionExtension
    {
        public static ServiceDescriptor GetDescriptor<TService>(this IServiceCollection services)
        {
            return services.GetDescriptor(typeof(TService));
        }

    }
}
