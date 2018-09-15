using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public static class ServiceDescriptorExtension
    {
        public static object CreateInstance(this ServiceDescriptor serviceDescriptor, IServiceProvider serviceProvider)
        {
            if (serviceDescriptor == null)
                throw new ArgumentNullException(nameof(serviceDescriptor));

            if (serviceDescriptor.ImplementationType != null)
                return serviceProvider.CreateInstance(serviceDescriptor.ImplementationType);
            if (serviceDescriptor.ImplementationFactory != null)
                return serviceDescriptor.ImplementationFactory(serviceProvider);
            return serviceDescriptor.ImplementationInstance;
        }
    }
}
