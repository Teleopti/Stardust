using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class GroupPageRepositoryTest : RepositoryTest<IGroupPage>
    {
        private GroupPageRepository _groupPageRepository;

        #region Default Testings
        /// <summary>
        /// Runs every test implemented by repositorie's concrete implementation
        /// </summary>
        protected override void ConcreteSetup()
        {
            _groupPageRepository = new GroupPageRepository(UnitOfWork);
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
            return new GroupPageRepository(currentUnitOfWork.Current());
        }
        #endregion

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
            IList<IGroupPage> groupPageCollection = _groupPageRepository.LoadAllGroupPageBySortedByDescription();

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
            Assert.AreEqual(person3, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection[1]);

            Assert.AreEqual(2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
            Assert.AreEqual(person5, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection[1]);


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
			IList<IGroupPage> groupPageCollection = _groupPageRepository.LoadAllGroupPageWhenPersonCollectionReAssociated();

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
			Assert.AreEqual(person3, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].PersonCollection[1]);

			Assert.AreEqual(2, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection.Count);
			Assert.AreEqual(person5, groupPageCollection[1].RootGroupCollection[0].ChildGroupCollection[0].ChildGroupCollection[0].PersonCollection[1]);
		}
    }
}

