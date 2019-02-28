using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class GroupPageRepositoryTest : RepositoryTest<IGroupPage>
    {
		/// <summary>
        /// Runs every test implemented by repositorie's concrete implementation
        /// </summary>
        protected override void ConcreteSetup()
        {
        }
		
        protected override IGroupPage CreateAggregateWithCorrectBusinessUnit()
        {
            IGroupPage ret = new GroupPage("GroupPage");
            return ret;
        }

        protected override void VerifyAggregateGraphProperties(IGroupPage loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase);
            Assert.AreEqual("GroupPage",
                           CreateAggregateWithCorrectBusinessUnit().Description.ToString());
        }

        protected override Repository<IGroupPage> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return GroupPageRepository.DONT_USE_CTOR(currentUnitOfWork.Current());
        }

		[Test]
	    public void ShouldLoadRootGroupsForPersonOnTopLevel()
	    {
			IPerson person1 = PersonFactory.CreatePerson("Person1");
			PersistAndRemoveFromUnitOfWork(person1);

			IGroupPage sortOrder2 = new GroupPage("Contract Basis Page");

			IRootPersonGroup root = new RootPersonGroup("unit1");
			IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
			IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");

			sortOrder2.AddRootPersonGroup(root);
			root.AddChildGroup(child1);
			child1.AddChildGroup(child2);

			root.AddPerson(person1);

			PersistAndRemoveFromUnitOfWork(sortOrder2);

			IGroupPage sortOrder1 = new GroupPage("AAA");
			IRootPersonGroup root2 = new RootPersonGroup("unit2");
			sortOrder1.AddRootPersonGroup(root2);
			PersistAndRemoveFromUnitOfWork(sortOrder1);

		    var result = GroupPageRepository.DONT_USE_CTOR(UnitOfWork).GetGroupPagesForPerson(person1.Id.GetValueOrDefault());

		    result.Should().Not.Be.Empty();
			var groupPage = result.SingleOrDefault();
			groupPage.RootGroupCollection.Should().Not.Be.Empty();
			var rootGroup = groupPage.RootGroupCollection.SingleOrDefault();
			rootGroup.Should().Not.Be.Null();
			rootGroup.Name.Should().Be.EqualTo(root.Name);

	    }

		[Test]
		public void ShouldLoadRootGroupsForPersonInSubGroup()
		{
			IPerson person1 = PersonFactory.CreatePerson("Person1");
			PersistAndRemoveFromUnitOfWork(person1);

			IGroupPage sortOrder2 = new GroupPage("Contract Basis Page");

			IRootPersonGroup root = new RootPersonGroup("unit1");
			IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
			IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");

			sortOrder2.AddRootPersonGroup(root);
			root.AddChildGroup(child1);
			child1.AddChildGroup(child2);

			child1.AddPerson(person1);

			PersistAndRemoveFromUnitOfWork(sortOrder2);

			IGroupPage sortOrder1 = new GroupPage("AAA");
			IRootPersonGroup root2 = new RootPersonGroup("unit2");
			sortOrder1.AddRootPersonGroup(root2);
			PersistAndRemoveFromUnitOfWork(sortOrder1);

			var result = GroupPageRepository.DONT_USE_CTOR(UnitOfWork).GetGroupPagesForPerson(person1.Id.GetValueOrDefault());

			result.Should().Not.Be.Empty();
			var groupPage = result.SingleOrDefault();
			groupPage.RootGroupCollection.Should().Not.Be.Empty();
			var rootGroup = groupPage.RootGroupCollection.SingleOrDefault();
			rootGroup.Should().Not.Be.Null();
			rootGroup.Name.Should().Be.EqualTo(root.Name);

		}

		[Test]
		public void ShouldLoadRootGroupsForPersonInSecondLevelSubGroup()
		{
			IPerson person1 = PersonFactory.CreatePerson("Person1");
			PersistAndRemoveFromUnitOfWork(person1);

			IGroupPage sortOrder2 = new GroupPage("Contract Basis Page");

			IRootPersonGroup root = new RootPersonGroup("unit1");
			IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
			IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");

			sortOrder2.AddRootPersonGroup(root);
			root.AddChildGroup(child1);
			child1.AddChildGroup(child2);

			child2.AddPerson(person1);

			PersistAndRemoveFromUnitOfWork(sortOrder2);

			IGroupPage sortOrder1 = new GroupPage("AAA");
			IRootPersonGroup root2 = new RootPersonGroup("unit2");
			sortOrder1.AddRootPersonGroup(root2);
			PersistAndRemoveFromUnitOfWork(sortOrder1);

			var result = GroupPageRepository.DONT_USE_CTOR(UnitOfWork).GetGroupPagesForPerson(person1.Id.GetValueOrDefault());

			result.Should().Not.Be.Empty();
			var groupPage = result.SingleOrDefault();
			groupPage.RootGroupCollection.Should().Not.Be.Empty();
			var rootGroup = groupPage.RootGroupCollection.SingleOrDefault();
			rootGroup.Should().Not.Be.Null();
			rootGroup.Name.Should().Be.EqualTo(root.Name);

		}

		[Test]
		public void ShouldLoadRootGroupsForPersonInThirdLevelSubGroup()
		{
			IPerson person1 = PersonFactory.CreatePerson("Person1");
			PersistAndRemoveFromUnitOfWork(person1);

			IGroupPage sortOrder2 = new GroupPage("Contract Basis Page");

			IRootPersonGroup root = new RootPersonGroup("unit1");
			IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
			IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");
			IChildPersonGroup child3 = new ChildPersonGroup("subSubSubUnit1");

			sortOrder2.AddRootPersonGroup(root);
			root.AddChildGroup(child1);
			child1.AddChildGroup(child2);

			child2.AddChildGroup(child3);

			child3.AddPerson(person1);

			PersistAndRemoveFromUnitOfWork(sortOrder2);

			IGroupPage sortOrder1 = new GroupPage("AAA");
			IRootPersonGroup root2 = new RootPersonGroup("unit2");
			sortOrder1.AddRootPersonGroup(root2);
			PersistAndRemoveFromUnitOfWork(sortOrder1);

			var result = GroupPageRepository.DONT_USE_CTOR(UnitOfWork).GetGroupPagesForPerson(person1.Id.GetValueOrDefault());

			result.Should().Not.Be.Empty();
			var groupPage = result.SingleOrDefault();
			groupPage.RootGroupCollection.Should().Not.Be.Empty();
			var rootGroup = groupPage.RootGroupCollection.SingleOrDefault();
			rootGroup.Should().Not.Be.Null();
			rootGroup.Name.Should().Be.EqualTo(root.Name);

		}

		[Test]
	    public void VerifyLoadGroupPagesByIds()
	    {
			IPerson person1 = PersonFactory.CreatePerson("Person1");
			IPerson person2 = PersonFactory.CreatePerson("Person2");
			IPerson person3 = PersonFactory.CreatePerson("Person3");
			IPerson person4 = PersonFactory.CreatePerson("Person4");
			IPerson person5 = PersonFactory.CreatePerson("Person5");


			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);
			PersistAndRemoveFromUnitOfWork(person4);
			PersistAndRemoveFromUnitOfWork(person5);

			IGroupPage sortOrder2 = new GroupPage("Contract Basis Page");

			IRootPersonGroup root = new RootPersonGroup("unit1");
			IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
			IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");

			sortOrder2.AddRootPersonGroup(root);
			root.AddChildGroup(child1);
			child1.AddChildGroup(child2);

			root.AddPerson(person1);

			child1.AddPerson(person2);
			child1.AddPerson(person3);

			child2.AddPerson(person4);
			child2.AddPerson(person5);

			PersistAndRemoveFromUnitOfWork(sortOrder2);
			IGroupPage sortOrder1 = new GroupPage("AAA");

			PersistAndRemoveFromUnitOfWork(sortOrder1);
			IList<IGroupPage> groupPageCollection = GroupPageRepository.DONT_USE_CTOR(UnitOfWork).LoadGroupPagesByIds(new[] { sortOrder2.Id.Value});

			Assert.AreEqual(groupPageCollection[0], sortOrder2);

			Assert.AreEqual(1, groupPageCollection[0].RootGroupCollection.Count);
			Assert.AreEqual(root, groupPageCollection[0].RootGroupCollection[0]);

			Assert.AreEqual(1, groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection.Count);
			Assert.AreEqual(child1, groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection[0]);

			Assert.AreEqual(1, groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection.Count);
			Assert.AreEqual(child2, groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0]);

			Assert.AreEqual(1, groupPageCollection[0].RootGroupCollection[0].PersonCollection.Count);
			Assert.AreEqual(person1, groupPageCollection[0].RootGroupCollection[0].PersonCollection[0]);

			Assert.AreEqual(2, groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
			groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection.Should().Contain(person3);

			Assert.AreEqual(2, groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
			groupPageCollection[0].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Should().Contain(person5);
	    }


	    [Test]
        public void VerifyLoadAllGroupPageBySortedByName()
        {
            IPerson person1 = PersonFactory.CreatePerson("Person1");
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            IPerson person3 = PersonFactory.CreatePerson("Person3");
            IPerson person4 = PersonFactory.CreatePerson("Person4");
            IPerson person5 = PersonFactory.CreatePerson("Person5");


            PersistAndRemoveFromUnitOfWork(person1);
            PersistAndRemoveFromUnitOfWork(person2);
            PersistAndRemoveFromUnitOfWork(person3);
            PersistAndRemoveFromUnitOfWork(person4);
            PersistAndRemoveFromUnitOfWork(person5);

            IGroupPage sortOrder2 = new GroupPage("Contract Basis Page");

            IRootPersonGroup root = new RootPersonGroup("unit1");
            IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
            IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");

            sortOrder2.AddRootPersonGroup(root);
            root.AddChildGroup(child1);
            child1.AddChildGroup(child2);

            root.AddPerson(person1);

            child1.AddPerson(person2);
            child1.AddPerson(person3);

            child2.AddPerson(person4);
            child2.AddPerson(person5);

            PersistAndRemoveFromUnitOfWork(sortOrder2);
            IGroupPage sortOrder1 = new GroupPage("AAA");

            PersistAndRemoveFromUnitOfWork(sortOrder1);
            IList<IGroupPage> groupPageCollection = GroupPageRepository.DONT_USE_CTOR(UnitOfWork).LoadAllGroupPageBySortedByDescription();

            Assert.AreEqual(groupPageCollection[0], sortOrder1);
            Assert.AreEqual(groupPageCollection[1], sortOrder2);

            Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection.Count);
            Assert.AreEqual(root, groupPageCollection[1].RootGroupCollection[0]);

            Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection.Count);
            Assert.AreEqual(child1, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0]);

            Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection.Count);
            Assert.AreEqual(child2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0]);

            Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection[0].PersonCollection.Count);
            Assert.AreEqual(person1, groupPageCollection[1].RootGroupCollection[0].PersonCollection[0]);

            Assert.AreEqual(2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
			groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection.Should().Contain(person3);

            Assert.AreEqual(2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
            groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Should().Contain(person5);
        }

		[Test]
		public void VerifyLoadAllGroupPageWhenPersonCollectionReAssociated()
		{
			IPerson person1 = PersonFactory.CreatePerson("Person1");
			IPerson person2 = PersonFactory.CreatePerson("Person2");
			IPerson person3 = PersonFactory.CreatePerson("Person3");
			IPerson person4 = PersonFactory.CreatePerson("Person4");
			IPerson person5 = PersonFactory.CreatePerson("Person5");


			PersistAndRemoveFromUnitOfWork(person1);
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(person3);
			PersistAndRemoveFromUnitOfWork(person4);
			PersistAndRemoveFromUnitOfWork(person5);

			IGroupPage sortOrder2 = new GroupPage("Contract Basis Page");

			IRootPersonGroup root = new RootPersonGroup("unit1");
			IChildPersonGroup child1 = new ChildPersonGroup("subUnit1");
			IChildPersonGroup child2 = new ChildPersonGroup("subSubUnit1");

			sortOrder2.AddRootPersonGroup(root);
			root.AddChildGroup(child1);
			child1.AddChildGroup(child2);

			root.AddPerson(person1);

			child1.AddPerson(person2);
			child1.AddPerson(person3);

			child2.AddPerson(person4);
			child2.AddPerson(person5);

			PersistAndRemoveFromUnitOfWork(sortOrder2);
			IGroupPage sortOrder1 = new GroupPage("AAA");

			PersistAndRemoveFromUnitOfWork(sortOrder1);
			IList<IGroupPage> groupPageCollection = GroupPageRepository.DONT_USE_CTOR(UnitOfWork).LoadAllGroupPageWhenPersonCollectionReAssociated();

            Assert.AreEqual(groupPageCollection[0], sortOrder1);
			Assert.AreEqual(groupPageCollection[1], sortOrder2);

			Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection.Count);
			Assert.AreEqual(root, groupPageCollection[1].RootGroupCollection[0]);

			Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection.Count);
			Assert.AreEqual(child1, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0]);

			Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection.Count);
			Assert.AreEqual(child2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0]);

			Assert.AreEqual(1, groupPageCollection[1].RootGroupCollection[0].PersonCollection.Count);
			Assert.AreEqual(person1, groupPageCollection[1].RootGroupCollection[0].PersonCollection[0]);

			Assert.AreEqual(2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
			groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection.Should().Contain(person3);

			Assert.AreEqual(2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
			groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Should().Contain(person5);
		}
    }
}

