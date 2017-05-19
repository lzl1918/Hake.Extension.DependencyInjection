using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class Out<T>
    {
        public T Value { get; set; }
        private Out()
        {

        }
        public static Out<T> Create()
        {
            return new Out<T>();
        }
    }

    public sealed class In<T>
    {
        public T Value { get; set; }
        private In(T value)
        {
            Value = value;
        }

        public static In<T> Create(T value)
        {
            return new In<T>(value);
        }
    }

    public sealed class Ref<T>
    {
        public T Value { get; set; }
        private Ref(ref T value)
        {
            Value = value;
        }

        public static Ref<T> Create(ref T value)
        {
            return new Ref<T>(ref value);
        }
    }
}
