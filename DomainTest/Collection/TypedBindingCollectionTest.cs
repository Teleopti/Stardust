using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class TypedBindingCollectionTest
    {
        private TypedBindingCollection<ForTypedBindingListTest> _list;

        [SetUp]
        public void Setup()
        {
            _list = new TypedBindingCollection<ForTypedBindingListTest>();
        }

        [Test]
        public void TestTheTest()
        {
            ForTypedBindingListTest test = new ForTypedBindingListTest();
            test.Test1 = "";
            test.Test2 = 2;
            test.List1 = new List<ForTypedBindingListTest>();
            Assert.AreEqual("", test.Test1);
            Assert.AreEqual(2, test.Test2);
            Assert.AreEqual(0, test.List1.Count);
            _list.Add(test);
        }

        [Test]
        public void VerifyGetItemProperties()
        {
            PropertyDescriptorCollection ret = _list.GetItemProperties(null);
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void VerifyGetItemProperties2()
        {
            ForTypedBindingListTest test = new ForTypedBindingListTest();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(test);

            // Sets an PropertyDescriptor to the specific property.
            PropertyDescriptor myProperty = properties.Find("Test1", false);
            PropertyDescriptor[] arr = new PropertyDescriptor[1];
            arr[0] = myProperty;

            PropertyDescriptorCollection ret = _list.GetItemProperties(arr);
            Assert.AreEqual(1, ret.Count);
        }

        [Test]
        public void VerifyGetItemProperties3()
        {
            ForTypedBindingListTest test = new ForTypedBindingListTest();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(test);

            // Sets an PropertyDescriptor to the specific property.
            System.ComponentModel.PropertyDescriptor myProperty = properties.Find("List1", false);
            PropertyDescriptor[] arr = new PropertyDescriptor[1];
            arr[0] = myProperty;

            PropertyDescriptorCollection ret = _list.GetItemProperties(arr);
            Assert.AreEqual(3, ret.Count);
        }


        [Test]
        public void VerifyGetItemProperties4()
        {
            ForTypedBindingListTest1 test = new ForTypedBindingListTest1();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(test);

            // Sets an PropertyDescriptor to the specific property.
            System.ComponentModel.PropertyDescriptor myProperty = properties.Find("Test9", false);
            PropertyDescriptor[] arr = new PropertyDescriptor[1];
            arr[0] = myProperty;

            PropertyDescriptorCollection ret = _list.GetItemProperties(arr);
            Assert.IsNull(ret);
        }


        [Test]
        public void VerifyGetListName()
        {
            Assert.AreEqual("List of ForTypedBindingListTest", _list.GetListName(null));
        }

        [Test]
        public void VerifyGettingPropertiesFormBaseAndInheritedOnInterfaces()
        {
            IChipmunk chipmunk = new Chipmunk();
            TypedBindingCollection<IChipmunk> bindingCollection = new TypedBindingCollection<IChipmunk>();
            bindingCollection.Add(chipmunk);
            PropertyDescriptorCollection descriptorCollection = bindingCollection.GetItemProperties(null);
            Assert.AreEqual(2, descriptorCollection.Count); //We should get both Hairy and Eyes
        }
    }

    internal class ForTypedBindingListTest
    {
        private string _test1;
        private int _test2;
        private List<ForTypedBindingListTest> list = new List<ForTypedBindingListTest>();

        public string Test1
        {
            get { return _test1; }
            set { _test1 = value; }
        }

        public int Test2
        {
            get { return _test2; }
            set { _test2 = value; }
        }

        public List<ForTypedBindingListTest> List1
        {
            get { return list; }
            set { list = value; }
        }
    }

    internal class ForTypedBindingListTest1
    {
        private string _test9;

        public string Test9
        {
            get { return _test9; }
            set { _test9 = value; }
        }
    }

    internal interface IRodent
    {
        string Hairy { get; set; }  
    }

    internal interface IChipmunk : IRodent
    {
        string Eyes { get; set; }
    }

    internal class Chipmunk : IChipmunk
    {
        public string Hairy
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Eyes
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
