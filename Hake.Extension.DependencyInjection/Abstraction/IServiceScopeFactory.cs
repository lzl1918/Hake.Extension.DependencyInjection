namespace Hake.Extension.DependencyInjection.Abstraction
{
    public interface IServiceScopeFactory
    {
        IServiceScope CreateScope();
    }
}
