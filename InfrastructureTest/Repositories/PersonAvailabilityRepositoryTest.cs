using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests PersonAvailabilityRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
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

        protected override Repository<IPersonAvailability> TestRepository(IUnitOfWork unitOfWork)
        {
            return new PersonAvailabilityRepository(unitOfWork);
        }

        [Test]
        public void VerifyCanFindBasedOnDateAndPerson()
        {
            IPersonAvailability org = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(org);

            IPerson person = PersonFactory.CreatePerson("p");
            PersistAndRemoveFromUnitOfWork(person);
            ICccTimeZoneInfo timeZone = person.PermissionInformation.DefaultTimeZone();

            var thePeriod = DateTimeFactory.CreateDateTimePeriod(
                timeZone.ConvertTimeToUtc(DateTime.SpecifyKind(org.StartDate, DateTimeKind.Unspecified), timeZone), 1);
            ICollection<IPersonAvailability> result = new PersonAvailabilityRepository(UnitOfWork).Find(new List<IPerson>{org.Person},thePeriod);
            Assert.AreEqual(1,result.Count);

            result = new PersonAvailabilityRepository(UnitOfWork).Find(new List<IPerson> { org.Person }, thePeriod.MovePeriod(TimeSpan.FromDays(2)));
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