using Hake.Extension.DependencyInjection.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Implementations.InternalImplementations
{
    internal sealed class ServiceCollection : IServiceCollection
    {
        private Dictionary<string, ServiceDescriptor> descriptorPool = new Dictionary<string, ServiceDescriptor>();

        public bool Add(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor == null)
                return false;

            Type serviceType = serviceDescriptor.ServiceType;
            string typeName = serviceType.FullName;
            if (descriptorPool.ContainsKey(typeName) == false)
            {
                descriptorPool.Add(typeName, serviceDescriptor);
                return true;
            }
            return false;
        }

        private bool disposed = false;
        ~ServiceCollection()
        {
            if (!disposed)
                return;
            Dispose();
        }
        public void Dispose()
        {
            if (disposed)
                return;

            foreach (var pair in descriptorPool)
            {
                if (pair.Value.ImplementationInstance == this)
                    continue;
                pair.Value.TryDispose();
            }
            disposed = true;
        }

        public void ExplicitAdd(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor == null)
                return;

            Type serviceType = serviceDescriptor.ServiceType;
            string typeName = serviceType.FullName;

            descriptorPool[typeName] = serviceDescriptor;
        }

        public ServiceDescriptor GetDescriptor(Type serviceType)
        {
            if (serviceType == null)
                return null;

            string typeName = serviceType.FullName;
            ServiceDescriptor descriptor = descriptorPool[typeName];
            return descriptor;
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
