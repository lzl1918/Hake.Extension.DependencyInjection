using System;
using System.Collections.Generic;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public interface IReadOnlyServiceCollection
    {
        ServiceDescriptor GetDescriptor(Type serviceType);
        bool TryGetDescriptor(Type serviceType, out ServiceDescriptor descriptor);
        IEnumerable<ServiceDescriptor> GetDescriptors();
    }
}
