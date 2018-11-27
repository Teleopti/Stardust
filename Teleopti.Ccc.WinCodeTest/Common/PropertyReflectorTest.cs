using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class PropertyReflectorTest
    {
        private PropertyReflector propertyReflector;

        [SetUp]
        public void Setup()
        {
            propertyReflector = new PropertyReflector();
        }

        [Test]
        public void TestGetValue()
        {
            CheckGetValue(GetTestObject());
        }

        [Test]
        public void TestSetValue()
        {
            CheckSetValue(GetTestObject());
        }

        [Test]
        public void TestSetValueWithNullAsCurrent()
        {
            MyTestObject testObject1 = new MyTestObject();
            MyTestObject testObject2 = new MyTestObject();
            propertyReflector.SetValue(testObject1, "Child", testObject2);
            propertyReflector.SetValue(testObject1, "Child.Name", "test");
            Assert.AreEqual(testObject2, testObject1.Child);
            Assert.AreEqual("test", testObject1.Child.Name);
        }

        [Test]
        public void TestDynamicProxy()
        {
            MyTestObject o = GetProxyTestObject();
            CheckGetValue(o);
            CheckSetValue(o);
        }

        [Test]
        public void TestType()
        {
            Type theType = propertyReflector.GetType(typeof(MyTestObject), "Child.Name");
            Assert.AreEqual(typeof(string), theType);

            theType = propertyReflector.GetType(typeof(MyTestObject), "Child");
            Assert.AreEqual(typeof(AbstractTestObject), theType);

            theType = propertyReflector.GetType(typeof(MyTestObject), "Name");
            Assert.AreEqual(typeof(string), theType);

            theType = propertyReflector.GetType(typeof(MyNewTestObject), "SecondChild.Name");
            Assert.AreEqual(typeof(string), theType);
        }

        private void CheckSetValue(MyTestObject o)
        {
            propertyReflector.SetValue(o, "Id", "1b");
            propertyReflector.SetValue(o, "Name", "OneB");
            propertyReflector.SetValue(o, "Child.Id", "2b");
            propertyReflector.SetValue(o, "Child.Name", "TwoB");
            Assert.AreEqual(o.Id, "1b");
            Assert.AreEqual(o.Name, "OneB");
            Assert.AreEqual(o.Child.Id, "2b");
            Assert.AreEqual(o.Child.Name, "TwoB");
        }

        private void CheckGetValue(MyTestObject o)
        {
            Assert.AreEqual(propertyReflector.GetValue(o, "Id"), o.Id);
            Assert.AreEqual(propertyReflector.GetValue(o, "Name"), o.Name);
            Assert.AreEqual(propertyReflector.GetValue(o, "Child.Id"), o.Child.Id);
            Assert.AreEqual(propertyReflector.GetValue(o, "Child.Name"), o.Child.Name);

            Assert.IsNull(propertyReflector.GetValue(o, "Child.Child"));
            Assert.IsNull(propertyReflector.GetValue(o, "Child.Child.Child.Id"));
        }

        private static MyTestObject GetTestObject()
        {
            return new MyTestObject("1", "One", new MyTestObject("2", "Two", null));
        }

        private static MyTestObject GetProxyTestObject()
        {
            MockRepository mocks = new MockRepository();
            MyTestObject o = mocks.StrictMock<MyTestObject>();
            MyTestObject o2 = mocks.StrictMock<MyTestObject>();

            SetupResult.For(o.Id).PropertyBehavior();
            SetupResult.For(o.Name).PropertyBehavior();
            SetupResult.For(o.Child).PropertyBehavior();
            SetupResult.For(o2.Id).PropertyBehavior();
            SetupResult.For(o2.Name).PropertyBehavior();
            SetupResult.For(o2.Child).PropertyBehavior();

            mocks.ReplayAll();

            o.Id = "1";
            o2.Id = "2";
            o.Name = "One";
            o2.Name = "Two";
            o.Child = o2;
            o2.Child = null;

            return o;
        }
    }


    public interface ITestObject
    {
        string Id { get; set; }

        string Name { get; set; }
    }

    public abstract class AbstractTestObject : ITestObject
    {
        private string id;

        protected AbstractTestObject() { }

        protected AbstractTestObject(string id)
        {
            this.id = id;
        }

        public virtual string Id
        {
            get { return id; }
            set { id = value; }
        }

        public abstract string Name { get; set; }
    }

    public class MyTestObject : AbstractTestObject
    {
        private string name;
        private AbstractTestObject child;

        public MyTestObject() { }

        public MyTestObject(string id, string name, AbstractTestObject child)
            : base(id)
        {
            this.name = name;
            this.child = child;
        }

        public override string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual AbstractTestObject Child
        {
            get { return child; }
            set { child = value; }
        }
    }

    public class MyNewTestObject : MyTestObject
    {
        public MyNewTestObject() { }

        private ITestObject secondChild;

        public virtual ITestObject SecondChild
        {
            get { return secondChild; }
            set { secondChild = value; }
        }
    }
}
