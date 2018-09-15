using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;

namespace Hake.Extension.DependencyInjection.Implementations.Internals
{
    internal sealed class ServiceCollection : IServiceCollection
    {
        private Dictionary<string, ServiceDescriptor> descriptorPool = new Dictionary<string, ServiceDescriptor>();

        public bool Add(ServiceDescriptor serviceDescriptor, bool replaceIfExists)
        {
            if (serviceDescriptor == null)
                return false;

            Type serviceType = serviceDescriptor.ServiceType;
            string typeName = serviceType.FullName;
            if (!descriptorPool.ContainsKey(typeName))
            {
                descriptorPool.Add(typeName, serviceDescriptor);
                return true;
            }
            if (!replaceIfExists)
                return false;
            descriptorPool.Add(typeName, serviceDescriptor);
            return true;
        }
        
        public ServiceDescriptor GetDescriptor(Type serviceType)
        {
            if (serviceType == null)
                return null;

            string typeName = serviceType.FullName;
            ServiceDescriptor descriptor = descriptorPool[typeName];
            return descriptor;
        }
        public bool TryGetDescriptor(Type serviceType, out ServiceDescriptor descriptor)
        {
            if (serviceType == null)
            {
                descriptor = null;
                return false;
            }
            string typeName = serviceType.FullName;
            return descriptorPool.TryGetValue(typeName, out descriptor);
        }
        public IEnumerable<ServiceDescriptor> GetDescriptors()
        {
            return descriptorPool.Values;
        }

        public bool Remove(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor == null)
                return false;

            Type serviceType = serviceDescriptor.ServiceType;
            string typeName = serviceType.FullName;
            ServiceDescriptor descInPool;
            if (descriptorPool.TryGetValue(typeName, out descInPool) == false)
                return false;
            if (descInPool == serviceDescriptor)
                return descriptorPool.Remove(typeName);
            else
                return false;
        }
    }
}
