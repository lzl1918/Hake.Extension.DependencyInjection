using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hake.Extension.DependencyInjection.Abstraction;
using Hake.Extension.DependencyInjection.Implementations;
using System.Linq;

namespace Test
{
    [TestClass]
    public class CollectionTest
    {
        [TestMethod]
        public void AddRemoveTest()
        {
            ServiceDescriptor descA = ServiceDescriptor.Transient<Int>(service => new Int(4));
            ServiceDescriptor descB = ServiceDescriptor.Singleton<Int>(service => new Int(4));
            ServiceDescriptor descC = ServiceDescriptor.Singleton<FakeA>();
            ServiceDescriptor descD = ServiceDescriptor.Singleton<IFake, FakeA>();
            ServiceDescriptor descE = ServiceDescriptor.Singleton<IFake, FakeB>();
            IServiceCollection pool = Implementation.CreateServiceCollection();
            Assert.AreEqual(true, pool.Add(descA));
            Assert.AreEqual(false, pool.Add(descA));
            Assert.AreEqual(false, pool.Add(descB));
            Assert.AreEqual(true, pool.Add(descC));
            Assert.AreEqual(true, pool.Add(descD));
            Assert.AreEqual(false, pool.Add(descE));
            Assert.AreEqual(3, pool.GetDescriptors().Count());

            Assert.AreEqual(false, pool.Remove(descB));
            Assert.AreEqual(true, pool.Remove(descA));
        }
        [TestMethod]
        public void GetTest()
        {
            ServiceDescriptor descA = ServiceDescriptor.Transient<Int>(service => new Int(4));
            ServiceDescriptor descB = ServiceDescriptor.Singleton<Int>(service => new Int(4));
            ServiceDescriptor descC = ServiceDescriptor.Singleton<FakeA>();
            ServiceDescriptor descD = ServiceDescriptor.Singleton<IFake, FakeA>();
            ServiceDescriptor descE = ServiceDescriptor.Singleton<IFake, FakeB>();
            IServiceCollection pool = Implementation.CreateServiceCollection();
            Assert.AreEqual(true, pool.Add(descA));
            Assert.AreEqual(false, pool.Add(descA));
            Assert.AreEqual(false, pool.Add(descB));
            Assert.AreEqual(true, pool.Add(descC));
            Assert.AreEqual(true, pool.Add(descD));
            Assert.AreEqual(false, pool.Add(descE));
            Assert.AreEqual(3, pool.GetDescriptors().Count());

            Assert.AreEqual(false, pool.Remove(descB));
            Assert.AreEqual(true, pool.Remove(descA));

            Assert.AreSame(descD, pool.GetDescriptor<IFake>());
            Assert.AreNotSame(descE, pool.GetDescriptor<IFake>());
        }
    }
}
