using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{

    /// <summary>
    /// Tests for GroupPageTest
    /// </summary>
    [TestFixture]
    public class GroupPageTest
    {
        private IGroupPage _page;

        [SetUp]
        public void Setup()
        {
            _page = new GroupPage("GroupPageInRockLand");
        }

        [Test]
        public void VerifyCanCreate()
        {
            _page = new GroupPage("Contract Basis Page");

            Assert.AreEqual(0, _page.RootGroupCollection.Count);
            Assert.IsNotNull(_page.RootGroupCollection);
            Assert.IsNotEmpty(_page.Description.ToString());
        }

        [Test]
        public void VerifyDescriptionCanSet()
        {
            Description newGroup = new Description("Funny Group");
            _page.Description = newGroup;

            Assert.AreEqual(newGroup, _page.Description);
        }
        [Test]
        public void VerifyRootIsAggregateRoot()
        {
            _page = new GroupPage("Contract Basis Page");

            IRootPersonGroup root = new RootPersonGroup("unit1");
            IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
            IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");

            _page.AddRootPersonGroup(root);
            root.AddChildGroup(child1);
            child1.AddChildGroup(child2);

            Assert.AreSame(_page, child2.Root());
            Assert.AreSame(_page, root.Parent);
            Assert.AreSame(root, child1.Parent);
            Assert.AreSame(child1, child2.Parent);
        }

        [Test]
        public void VerifyGroupUnitCanCreateRemove()
        {
            // Create persons for group unit
            IPerson person1 = PersonFactory.CreatePerson("Im for group unit1");
            IPerson person2 = PersonFactory.CreatePerson("Im for group unit1");

            IList<IPerson> personCollection = new List<IPerson>();
            personCollection.Add(person1);
            personCollection.Add(person2);

            // Create groupUnits
            IRootPersonGroup groupUnit1 = new RootPersonGroup("TopGroupUnit1");
            groupUnit1.AddPerson(person1);
            groupUnit1.AddPerson(person2);

            IRootPersonGroup groupUnit2 = new RootPersonGroup("TopGroupUnit2");

            // Check person collection in group units
            Assert.IsNotNull(groupUnit1.PersonCollection);
            Assert.AreEqual(2, groupUnit1.PersonCollection.Count);

           
            Assert.IsNotNull(groupUnit2.PersonCollection);
            Assert.AreEqual(0, groupUnit2.PersonCollection.Count);

            // Check child collection in group units
            Assert.IsNotNull(groupUnit1.ChildGroupCollection);
            Assert.IsNotNull(groupUnit2.ChildGroupCollection);
            
            // Add Group units to group page
            _page.AddRootPersonGroup(groupUnit1);
            _page.AddRootPersonGroup(groupUnit2);

            // Check scenario when group unit already exist in child collection
            _page.AddRootPersonGroup(groupUnit2);

            // Check group unit collection 
            Assert.AreEqual(2, _page.RootGroupCollection.Count);
            Assert.AreEqual(groupUnit1, _page.RootGroupCollection[0]);

            // Remove Group unit from group page
            _page.RemoveRootPersonGroup(groupUnit1);


            // Check group unit collection
            Assert.AreEqual(1, _page.RootGroupCollection.Count);
            Assert.AreEqual(groupUnit2, _page.RootGroupCollection[0]);
        }

        [Test]
        public void VerifyChildPersonGroupHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(ChildPersonGroup),true));
        }

        [Test]
        public void ShouldBeAbleToOverrideGroupPageName()
        {
            string originalDescription = _page.Description.Name;
            Assert.AreEqual(originalDescription, _page.RootNodeName);
        }

		[Test]
		public void ShouldBeUserDefinedIfNoDescriptionKey()
		{
			_page.DescriptionKey = null;

			Assert.True(_page.IsUserDefined());
		}

		[Test]
		public void ShouldBeAbleToVerifyNotBuiltInGroupPage()
		{
			_page.DescriptionKey = "keyhihi";

			Assert.False(_page.IsUserDefined());
		}
    }
}
