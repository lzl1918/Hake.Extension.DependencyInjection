using Hake.Extension.DependencyInjection.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    [TestClass]
    public class ServiceDescriptorTest
    {
        [TestMethod]
        public void TransientTest()
        {
            Int instance = new Int(10);
            ServiceDescriptor descA = ServiceDescriptor.Transient<Int>(service => new Int(4));
            Assert.AreNotSame(instance, descA.GetInstance());
            Assert.AreNotSame(descA.GetInstance(), descA.GetInstance());

            ServiceDescriptor descB = ServiceDescriptor.Transient<Int>();
            Assert.AreEqual(0, (descB.GetInstance() as Int).Value);
            Assert.AreNotSame(descB.GetInstance(), descB.GetInstance());
            Assert.AreEqual((descB.GetInstance() as Int).Value, (descB.GetInstance() as Int).Value);

            ServiceDescriptor descC = ServiceDescriptor.Transient<IFake, FakeA>();
            ServiceDescriptor descD = ServiceDescriptor.Transient<IFake, FakeB>();
            Assert.AreNotEqual(descC.GetInstance().GetType(), descD.GetInstance().GetType());
            Assert.AreEqual(1, (descC.GetInstance() as IFake).Value);
            Assert.AreEqual(2, (descD.GetInstance() as IFake).Value);
        }

        [TestMethod]
        public void SingletonTest()
        {
            Int instance = new Int(10);
            ServiceDescriptor descA = ServiceDescriptor.Singleton<Int>(instance);
            ServiceDescriptor descB = ServiceDescriptor.Singleton<Int>(service => new Int(4));
            Assert.AreSame(instance, descA.GetInstance());
            Assert.AreSame(descB.GetInstance(), descB.GetInstance());

            ServiceDescriptor descC = ServiceDescriptor.Singleton<Int>();
            Assert.AreEqual(0, (descC.GetInstance() as Int).Value);
            Assert.AreSame(descC.GetInstance(), descC.GetInstance());
            Int tempInstance = descC.GetInstance() as Int;

            descA.EnterScope();
            Assert.AreSame(instance, descA.GetInstance());
            descA.ExitScope();
            Assert.AreSame(instance, descA.GetInstance());

            descC.EnterScope();
            Assert.AreSame(tempInstance, descC.GetInstance());
            descC.ExitScope();
            Assert.AreSame(tempInstance, descC.GetInstance());

            ServiceDescriptor descD = ServiceDescriptor.Singleton<IFake, FakeA>();
            ServiceDescriptor descE = ServiceDescriptor.Singleton<IFake, FakeB>();
            Assert.AreEqual(1, (descD.GetInstance() as IFake).Value);
            Assert.AreEqual(2, (descE.GetInstance() as IFake).Value);
        }

        [TestMethod]
        public void ScopedTest()
        {
            Int instance = new Int(10);
            ServiceDescriptor descA = ServiceDescriptor.Scoped<Int>(service => new Int(4));
            Assert.AreNotSame(instance, descA.GetInstance());
            Assert.AreSame(descA.GetInstance(), descA.GetInstance());

            ServiceDescriptor descB = ServiceDescriptor.Scoped<Int>();
            Assert.AreEqual(0, (descB.GetInstance() as Int).Value);
            Assert.AreSame(descB.GetInstance(), descB.GetInstance());

            ServiceDescriptor descC = ServiceDescriptor.Scoped<IFake, FakeA>();
            ServiceDescriptor descD = ServiceDescriptor.Scoped<IFake, FakeB>();
            Assert.AreNotEqual(descC.GetInstance().GetType(), descD.GetInstance().GetType());
            Assert.AreEqual(1, (descC.GetInstance() as IFake).Value);
            Assert.AreEqual(2, (descD.GetInstance() as IFake).Value);

            instance = descA.GetInstance() as Int;
            Assert.AreSame(instance, descA.GetInstance());
            descA.EnterScope();
            Assert.AreSame(instance, descA.GetInstance());
            descA.ExitScope();
            Assert.AreNotSame(instance, descA.GetInstance());
            Assert.AreEqual(4, (descA.GetInstance() as Int).Value);
        }
    }
}
