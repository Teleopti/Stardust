using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("BucketB")]
    public class LazyLoadingManagerTest : DatabaseTest
    {
        protected override void SetupForRepositoryTest()
        {
        }

        [Test]
        public void VerifyIsInitialized()
        {
            IPerson person = PersonFactory.CreatePerson();
            person.SetName(new Name("for", "test"));
            PersistAndRemoveFromUnitOfWork(person);

            person = Session.Load<Person>(person.Id.Value);
            Assert.IsFalse(LazyLoadingManager.IsInitialized(person));
            Assert.AreEqual("for", person.Name.FirstName);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(person));
        }

        [Test]
        public void VerifyInitializeOnEntity()
        {
            IPerson person = PersonFactory.CreatePerson();
            person.SetName(new Name("for", "test"));
            PersistAndRemoveFromUnitOfWork(person);

            person = Session.Load<Person>(person.Id.Value);
            Assert.IsFalse(LazyLoadingManager.IsInitialized(person));
            LazyLoadingManager.Initialize(person);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(person));
        }

        [Test]
        public void VerifyInitializeWorksWithLayerWrapperCollection()
        {
            IShiftCategory shiftCat = ShiftCategoryFactory.CreateShiftCategory("sdf");
            PersistAndRemoveFromUnitOfWork(shiftCat);
            IActivity act = new Activity("test");
            PersistAndRemoveFromUnitOfWork(act);
            IPerson per = PersonFactory.CreatePerson("hola23423423");
            PersistAndRemoveFromUnitOfWork(per);
            IScenario scen = ScenarioFactory.CreateScenarioAggregate();
            scen.ChangeName("scen");
            PersistAndRemoveFromUnitOfWork(scen);
            IPersonAssignment pAss =
                PersonAssignmentFactory.CreateAssignmentWithMainShift(per, scen, act, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), shiftCat);

            new PersonAssignmentRepository(CurrUnitOfWork).Add(pAss);
			Session.Flush();
			Session.Clear();

            pAss = Session.Load<PersonAssignment>(pAss.Id.Value);
						Assert.IsFalse(LazyLoadingManager.IsInitialized(pAss.ShiftLayers));
            LazyLoadingManager.Initialize(pAss.ShiftLayers);
						Assert.IsTrue(LazyLoadingManager.IsInitialized(pAss.ShiftLayers));
        }

        [Test]
        public void VerifyInitializeNullDoesNotCrash()
        {
            LazyLoadingManager.Initialize(null);
        }

        [Test]
        public void VerifyIsInitializedWhenNullReturnsTrue()
        {
            Assert.IsTrue(LazyLoadingManager.IsInitialized(null));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void WrapperShouldHaveCoverage()
		{
			var wrapper = new LazyLoadingManagerWrapper();
			wrapper.Initialize(null);
			wrapper.IsInitialized(null);
		}
    }
}
