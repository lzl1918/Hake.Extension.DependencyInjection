using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public class Int
    {
        public int Value { get; set; }
        public Int(int value = 0) { Value = value; }

        public int Change(int value)
        {
            Value = value;
            return value;
        }
        public void TryThrow()
        {
            throw new Exception("content defined");
        }

        public string OptionalParameters(params object[] param)
        {
            return "objects: " + param.Length;
        }
    }

    public interface IFake
    {
        int Value { get; }
    }

    public class FakeA : IFake
    {
        public int Value { get { return 1; } }
    }
    public class FakeB : IFake
    {
        public int Value { get { return 2; } }
    }

    public class MethodTests
    {
        public int ArraySize(int[] array)
        {
            return array.Length;
        }
        public int ListSize(IList<int> list)
        {
            return list.Count;
        }
        public int ArraySum(int[] array)
        {
            return array.Sum();
        }
        public int ListSum(IEnumerable<int> list)
        {
            return list.Sum();
        }
    }

    public class TakeArguments
    {
        public int FakeValue { get; }
        public int IntValue { get; }
        public string Match { get; }
        public int TestInt { get; }
        public int TestB { get; }
        public TakeArguments(IFake fake, Int val, string match, int testint = 0, int testb = 1)
        {
            FakeValue = fake.Value;
            IntValue = val.Value;
            Match = match;
            TestInt = testint;
            TestB = testb;
        }
    }

    public static class StaticObject
    { }

    public class PrivateObject
    {
        public int Value { get; }
        private PrivateObject(int value = 10)
        {
            Value = value;
        }
    }
    public abstract class AbstractObject
    {
    }

    public struct StructObject
    {
        public int Value { get; }
        public StructObject(int value = 10)
        {
            Value = value;
        }
    }

    public class GenericObject<T>
    {
        public T Value { get; }
        public GenericObject(T value = default(T))
        {
            Value = value;
        }
    }

    public enum EnumTest
    {
        A, B, C
    }
    public delegate void DelegateTest();
}
