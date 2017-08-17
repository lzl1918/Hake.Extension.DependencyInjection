using Hake.Extension.DependencyInjection.Abstraction;
using Hake.Extension.DependencyInjection.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    [TestClass]
    public class ObjectFactoryTest
    {
        [TestMethod]
        public void CreateObjectTest()
        {
            Int val = ObjectFactory.CreateInstance<Int>();
            Assert.AreEqual(0, val.Value);
        }


        [TestMethod]
        public void CreateObjectByArgumentsTest()
        {
            ServiceDescriptor descA = ServiceDescriptor.Singleton<Int>(service => new Int(10));
            ServiceDescriptor descB = ServiceDescriptor.Singleton<IFake, FakeA>();
            IServiceCollection pool = Implementation.CreateServiceCollection();
            pool.Add(descA);
            pool.Add(descB);
            IServiceProvider services = Implementation.CreateServiceProvider(pool);
            TakeArguments args = services.CreateInstance<TakeArguments>("match", 4, 0);
            Assert.AreEqual(1, args.FakeValue);
            Assert.AreEqual(10, args.IntValue);
            Assert.AreEqual("match", args.Match);
            Assert.AreEqual(4, args.TestInt);
            Assert.AreEqual(0, args.TestB);

            args = services.CreateInstance<TakeArguments>("match", 4);
            Assert.AreEqual(1, args.FakeValue);
            Assert.AreEqual(10, args.IntValue);
            Assert.AreEqual("match", args.Match);
            Assert.AreEqual(4, args.TestInt);
            Assert.AreEqual(1, args.TestB);

            args = services.CreateInstance<TakeArguments>(new Dictionary<string, object>()
            {
                ["match"] = "test_match"
            }, 4, 0);
            Assert.AreEqual(1, args.FakeValue);
            Assert.AreEqual(10, args.IntValue);
            Assert.AreEqual("test_match", args.Match);
            Assert.AreEqual(4, args.TestInt);
            Assert.AreEqual(0, args.TestB);

            args = services.CreateInstance<TakeArguments>(new Dictionary<string, object>()
            {
                ["match"] = "test_match",
                ["testb"] = 10
            });
            Assert.AreEqual(1, args.FakeValue);
            Assert.AreEqual(10, args.IntValue);
            Assert.AreEqual("test_match", args.Match);
            Assert.AreEqual(0, args.TestInt);
            Assert.AreEqual(10, args.TestB);

            args = services.CreateInstance<TakeArguments>(new Dictionary<string, object>()
            {
                ["match"] = "test_match",
                ["testb"] = 10
            }, "match", 4, 5);
            Assert.AreEqual(1, args.FakeValue);
            Assert.AreEqual(10, args.IntValue);
            Assert.AreEqual("test_match", args.Match);
            Assert.AreEqual(4, args.TestInt);
            Assert.AreEqual(10, args.TestB);
        }


        [TestMethod]
        public void CreateObjectsTest()
        {
            IServiceCollection pool = Implementation.CreateServiceCollection();
            IServiceProvider services = Implementation.CreateServiceProvider(pool);
            try
            {
                object staticobj = services.CreateInstance(typeof(StaticObject), 10) as StaticObject;
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                IFake fakeobj = services.CreateInstance<IFake>();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                AbstractObject absobj = services.CreateInstance<AbstractObject>();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                PrivateObject privateobj = services.CreateInstance<PrivateObject>(5);
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                EnumTest enumobj = services.CreateInstance<EnumTest>();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                DelegateTest delegateobj = services.CreateInstance<DelegateTest>();
                Assert.Fail();
            }
            catch (Exception)
            {
            }

            GenericObject<int> intgeneric = services.CreateInstance<GenericObject<int>>(10);
            Assert.AreEqual(10, intgeneric.Value);

            GenericObject<double> doublegeneric = services.CreateInstance<GenericObject<double>>();
            Assert.AreEqual(0, doublegeneric.Value);

            try
            {
                int[] intarray = services.CreateInstance<int[]>(10);
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            int inttest = services.CreateInstance<int>(5);
            Assert.AreEqual(5, inttest);

            StructObject structobj = services.CreateInstance<StructObject>(5);
            Assert.AreEqual(5, structobj.Value);
        }

        [TestMethod]
        public void InvokeMethodTest()
        {
            Int val = ObjectFactory.CreateInstance<Int>();
            Assert.AreEqual(0, val.Value);
            object ret = ObjectFactory.InvokeMethod(val, nameof(val.Change), 10);
            Assert.AreEqual(10, ret);
            Assert.AreEqual(10, val.Value);

            try
            {
                ObjectFactory.InvokeMethod(val, nameof(val.TryThrow));
            }
            catch (Exception ex) when (ex.Message == "content defined")
            {

            }

            ret = ObjectFactory.InvokeMethod(val, nameof(val.OptionalParameters), "str", 1, 2, 3, 4);
            Assert.AreEqual("objects: 5", ret);
        }

        [TestMethod]
        public void ValueOfArrayParameterTest()
        {
            MethodTests obj = new MethodTests();
            object ret;

            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySize));
            Assert.AreEqual(0, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySum));
            Assert.AreEqual(0, ret);

            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySize), new int[] { 1, 2, 3 });
            Assert.AreEqual(3, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySum), new int[] { 1, 2, 3 });
            Assert.AreEqual(6, ret);

            Dictionary<string, object> param = new Dictionary<string, object>()
            {
                ["array"] = new int[] { 1, 2, 3 }
            };

            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySize), param);
            Assert.AreEqual(3, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySum), param);
            Assert.AreEqual(6, ret);

            param["array"] = 10;
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySize), param);
            Assert.AreEqual(1, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySum), param);
            Assert.AreEqual(10, ret);
        }
        [TestMethod]
        public void ValueOfListParameterTest()
        {
            MethodTests obj = new MethodTests();
            object ret;

            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSize));
            Assert.AreEqual(0, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSum));
            Assert.AreEqual(0, ret);

            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSize), new int[] { 1, 2, 3 });
            Assert.AreEqual(3, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSum), new int[] { 1, 2, 3 });
            Assert.AreEqual(6, ret);

            List<int> lst = new List<int>() { 1, 2, 3 };
            Dictionary<string, object> param = new Dictionary<string, object>()
            {
                ["list"] = lst,
                ["array"] = lst,
            };

            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSize), param);
            Assert.AreEqual(3, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSum), param);
            Assert.AreEqual(6, ret);

            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySize), param);
            Assert.AreEqual(3, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ArraySum), param);
            Assert.AreEqual(6, ret);

            param["list"] = 10;
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSize), param);
            Assert.AreEqual(1, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSum), param);
            Assert.AreEqual(10, ret);

            param["list"] = new string[] { "1", "2", "3", "a" };
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSize), param);
            Assert.AreEqual(3, ret);
            ret = ObjectFactory.InvokeMethod(obj, nameof(obj.ListSum), param);
            Assert.AreEqual(6, ret);
        }
    }
}
