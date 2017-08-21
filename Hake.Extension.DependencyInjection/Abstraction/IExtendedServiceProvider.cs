using System;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public interface IExtendedServiceProvider : IServiceProvider
    {
        bool TryGetService(Type type, out object instance);
    }
}
