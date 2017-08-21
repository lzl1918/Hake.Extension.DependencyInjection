using System;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class MethodInvokeContext
    {
        private static readonly Type VOID_TYPE = typeof(void);

        public double Score { get; }
        public object[] Arguments { get; }
        public MethodBase Method { get; }

        internal MethodInvokeContext(double score, object[] arguments, MethodBase method)
        {
            Score = score;
            Arguments = arguments;
            Method = method;
        }

        public object Invoke(object instance)
        {
            if (Method is ConstructorInfo)
                return Invoke();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            try
            {
                object returnValue = Method.Invoke(instance, Arguments);
                if ((Method as MethodInfo).ReturnType != VOID_TYPE)
                    return returnValue;
                else
                    return null;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            catch
            {
                throw;
            }
        }
        public object Invoke()
        {
            if (Method is ConstructorInfo constructor)
            {
                try
                {
                    object returnValue = constructor.Invoke(Arguments);
                    return returnValue;
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
                catch
                {
                    throw;
                }
            }
            else
                throw new InvalidOperationException("method is not a constructor");
        }
    }
}
