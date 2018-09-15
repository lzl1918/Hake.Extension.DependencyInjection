using System;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public interface IServiceProviderFactory
    {
        IServiceProvider CreateServiceProvider(IReadOnlyServiceCollection serviceCollection);
    }
}
