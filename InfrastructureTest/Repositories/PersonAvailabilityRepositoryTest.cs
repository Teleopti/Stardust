using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class PersonAvailabilityRepositoryTest : RepositoryTest<IPersonAvailability>
    {
        private IPerson _person;
        private AvailabilityRotation _availability;
        private DateOnly _startDate;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _person = PersonFactory.CreatePerson("o");
            _availability = new AvailabilityRotation("Two weeks", 2 * 7);
            _startDate = new DateOnly(2008, 6, 30);

            PersistAndRemoveFromUnitOfWork(_person);
            PersistAndRemoveFromUnitOfWork(_availability);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IPersonAvailability CreateAggregateWithCorrectBusinessUnit()
        {
            return new PersonAvailability(_person, _availability, _startDate);
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IPersonAvailability loadedAggregateFromDatabase)
        {
            IPersonAvailability org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
            Assert.AreEqual(org.StartDate, loadedAggregateFromDatabase.StartDate);
            Assert.AreEqual(org.StartDay, loadedAggregateFromDatabase.StartDay);
            Assert.AreEqual(org.Availability, loadedAggregateFromDatabase.Availability);
        }

        protected override Repository<IPersonAvailability> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new PersonAvailabilityRepository(currentUnitOfWork.Current());
        }

        [Test]
        public void VerifyCanFindBasedOnDateAndPerson()
        {
            IPersonAvailability org = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(org);

            IPerson person = PersonFactory.CreatePerson("p");
            PersistAndRemoveFromUnitOfWork(person);
            
            var thePeriod = new DateOnlyPeriod(org.StartDate, org.StartDate);
            ICollection<IPersonAvailability> result = new PersonAvailabilityRepository(UnitOfWork).Find(new List<IPerson>{org.Person},thePeriod);
            Assert.AreEqual(1,result.Count);

	        result = new PersonAvailabilityRepository(UnitOfWork).Find(new List<IPerson> {org.Person},
		        new DateOnlyPeriod(thePeriod.StartDate.AddDays(1), thePeriod.EndDate.AddDays(1)));
            Assert.AreEqual(0, result.Count);

            result = new PersonAvailabilityRepository(UnitOfWork).Find(new List<IPerson> { person }, thePeriod);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void VerifyLoadWithoutLazyLoading()
        {
            IPersonAvailability newPersonAvail1 = CreateAggregateWithCorrectBusinessUnit();
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            IPersonAvailability newPersonAvail2 = new PersonAvailability(person2, _availability, _startDate);

            PersistAndRemoveFromUnitOfWork(person2);
            PersistAndRemoveFromUnitOfWork(newPersonAvail1);
            PersistAndRemoveFromUnitOfWork(newPersonAvail2);

			var testList = new PersonAvailabilityRepository(UnitOfWork).LoadPersonAvailabilityWithHierarchyData(new[] { person2 }, _startDate);

			Assert.IsTrue(LazyLoadingManager.IsInitialized(testList.First().Availability));
        }
    }
}