using System;
using System.Reflection;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class ValueMatchingEventArgs : EventArgs
    {
        public Type TargetType { get; }
        public Type InputType { get; }
        public object InputValue { get; }

        public bool Handled { get; private set; }
        internal object Value { get; private set; }

        internal ValueMatchingEventArgs(Type targetType, Type inputType, object inputValue)
        {
            TargetType = targetType;
            InputType = inputType;
            InputValue = inputValue;
        }

        public void SetValue(object value)
        {
            if (Handled)
                throw new Exception("cannot set value in mutiple times");
            Handled = true;
            Value = value;
        }
        internal void ClearFlags()
        {
            Handled = false;
            Value = null;
        }
    }
}
