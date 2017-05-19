using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public static class ServiceDescriptorExtension
    {
        public static void EnterScope(this ServiceDescriptor descriptor)
        {
            descriptor.NotifyScopeEntered();
        }
        public static void ExitScope(this ServiceDescriptor descriptor)
        {
            descriptor.NotifyScopeExited();
        }
    }
}
