using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public enum ServiceLifetime
    {
        Singleton = 0,
        Scoped = 1,
        Transient = 2
    }
}
