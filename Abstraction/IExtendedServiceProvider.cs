using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public interface IExtendedServiceProvider : IServiceProvider
    {
        bool TryGetService(Type type, out object instance);
    }
}
