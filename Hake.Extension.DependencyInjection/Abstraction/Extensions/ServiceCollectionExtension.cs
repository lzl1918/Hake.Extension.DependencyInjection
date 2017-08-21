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

        public static void EnterScope(this IServiceCollection services)
        {
            foreach (ServiceDescriptor descriptor in services.GetDescriptors())
                descriptor.NotifyScopeEntered();
        }
        public static void LeaveScope(this IServiceCollection services)
        {
            foreach (ServiceDescriptor descriptor in services.GetDescriptors())
                descriptor.NotifyScopeExited();
        }
    }
}
