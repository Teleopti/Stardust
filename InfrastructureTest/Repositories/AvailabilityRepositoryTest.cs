using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests AvailabilityRepository
    ///</summary>
    [TestFixture]
    [Category("BucketB")]
    public class AvailabilityRepositoryTest : RepositoryTest<IAvailabilityRotation>
    {

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }


        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IAvailabilityRotation CreateAggregateWithCorrectBusinessUnit()
        {
            return new AvailabilityRotation("My availability", 2 * 7);
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IAvailabilityRotation loadedAggregateFromDatabase)
        {
            IAvailabilityRotation org = CreateAggregateWithCorrectBusinessUnit();
            
            //Assert.AreEqual(org.DaysCount, loadedAggregateFromDatabase.DaysCount);
            Assert.AreEqual(org.Name,loadedAggregateFromDatabase.Name);
        }

        protected override Repository<IAvailabilityRotation> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return AvailabilityRepository.DONT_USE_CTOR(currentUnitOfWork.Current());
        }

        [Test]
        public void VerifyLoadAllWithAvailabilityAndAvailabilityDays()
        {
            //ShiftCategory shiftCategory = new ShiftCategory("sdf");
            //PersistAndRemoveFromUnitOfWork(shiftCategory);

            IAvailabilityRotation availability = CreateAggregateWithCorrectBusinessUnit();
            
            availability.FindAvailabilityDay(10).Restriction.WorkTimeLimitation =
                new WorkTimeLimitation(new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0));

            availability.FindAvailabilityDay(10).Restriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6, 0, 0), new TimeSpan(8, 0, 0));
            availability.FindAvailabilityDay(10).Restriction.EndTimeLimitation = new EndTimeLimitation(new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0));

            PersistAndRemoveFromUnitOfWork(availability);

            IList<IAvailabilityRotation> res = new List<IAvailabilityRotation>(AvailabilityRepository.DONT_USE_CTOR(UnitOfWork).LoadAllAvailabilitiesWithHierarchyData());

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(res[0].AvailabilityDays[0].Restriction));
            Assert.AreEqual(14, res[0].AvailabilityDays.Count);
            Assert.IsNotNull(res[0].AvailabilityDays[7].Restriction);
        }

		[Test]
		public void CanPersistNullAsRestriction()
		{
			var avail = new AvailabilityRotation("test", 2);
			avail.AddDays(1);
			avail.AvailabilityDays[0].Restriction.EndTimeLimitation = new EndTimeLimitation(null, null);
			PersistAndRemoveFromUnitOfWork(avail);

			var loaded = AvailabilityRepository.DONT_USE_CTOR(UnitOfWork).Get(avail.Id.Value);
			loaded.AvailabilityDays[0].Restriction.StartTimeLimitation.StartTime.HasValue
				.Should().Be.False();
				
		}

        [Test]
        public void VerifyPersistWhenDaysAreRemoved()
        {
            IAvailabilityRotation availability = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(availability);   // Persist instance (OK).
            availability = Session.Get<AvailabilityRotation>(availability.Id);
            Assert.AreEqual(14, availability.DaysCount);
            availability.RemoveDays(7);                     // Removes few days from rotation.
            PersistAndRemoveFromUnitOfWork(availability);   
            Assert.AreEqual(7, Session.Get<AvailabilityRotation>(availability.Id).DaysCount);
        }

        [Test]
        public void CanPersistAvailabilityWithoutRestriction()
        {
            var avail = new AvailabilityRotation("test", 1);
            avail.AvailabilityDays[0].Restriction = null;
            PersistAndRemoveFromUnitOfWork(avail);

            UnitOfWork.Clear();
            Assert.IsNull(Session.Get<AvailabilityRotation>(avail.Id).AvailabilityDays[0].Restriction);
        }

        [Test]
        public void ShouldLoadAggregateFromId()
        {
            var aggregate = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(aggregate);

            var loaded = AvailabilityRepository.DONT_USE_CTOR(UnitOfWork).LoadAggregate(aggregate.Id.Value);
            Assert.IsNotNull(loaded);
        }

        [Test]
        public void ShouldLoadAllAvailabilitiesSortedByNameAscending()
        {
            var avail1 = new AvailabilityRotation("test", 1);
            var avail2 = new AvailabilityRotation("atest", 1);
            var avail3 = new AvailabilityRotation("ytest", 1);
            PersistAndRemoveFromUnitOfWork(avail1);
            PersistAndRemoveFromUnitOfWork(avail2);
            PersistAndRemoveFromUnitOfWork(avail3);

            var availabilityRotations = AvailabilityRepository.DONT_USE_CTOR(UnitOfWork).LoadAllSortedByNameAscending().ToList();
            availabilityRotations[0].Name.Should().Be.EqualTo("atest");
            availabilityRotations[1].Name.Should().Be.EqualTo("test");
            availabilityRotations[2].Name.Should().Be.EqualTo("ytest");
        }
    }
}
