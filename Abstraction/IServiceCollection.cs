using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public interface IServiceCollection : IDisposable
    {
        bool Add(ServiceDescriptor serviceDescriptor);
        void ExplicitAdd(ServiceDescriptor serviceDescriptor);
        bool Remove(ServiceDescriptor serviceDescriptor);

        ServiceDescriptor GetDescriptor(Type serviceType);
        IEnumerable<ServiceDescriptor> GetDescriptors();
    }
}
