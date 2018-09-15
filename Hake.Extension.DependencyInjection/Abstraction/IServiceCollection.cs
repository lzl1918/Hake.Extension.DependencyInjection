using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public interface IServiceCollection : IReadOnlyServiceCollection
    {
        bool Add(ServiceDescriptor serviceDescriptor, bool replaceIfExists = false);
        bool Remove(ServiceDescriptor serviceDescriptor);
    }
}
