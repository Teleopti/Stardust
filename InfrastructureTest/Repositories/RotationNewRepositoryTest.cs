using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests RotationRepository
    ///</summary>
    [TestFixture]
    [Category("BucketB")]
    public class RotationNewRepositoryTest : RepositoryTest<IRotation>
    {
        private IShiftCategory _shiftCategory;
        private IDayOffTemplate _dayOff;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("sdf");
            PersistAndRemoveFromUnitOfWork(_shiftCategory);
            _dayOff = DayOffFactory.CreateDayOff(new Description("sgksa"));
            PersistAndRemoveFromUnitOfWork(_dayOff);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IRotation CreateAggregateWithCorrectBusinessUnit()
        {
            Rotation rotation = new Rotation("Rotation 1",3*7);

            StartTimeLimitation startTimeLimitation = new StartTimeLimitation(new TimeSpan(0, 0, 0), new TimeSpan(8, 0, 0));
            rotation.RotationDays[10].RestrictionCollection[0].WorkTimeLimitation =
                new WorkTimeLimitation(new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0));
            rotation.RotationDays[10].RestrictionCollection[0].StartTimeLimitation = startTimeLimitation;

            return rotation;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IRotation loadedAggregateFromDatabase)
        {
            IRotation org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(org.DaysCount, loadedAggregateFromDatabase.DaysCount);
            
            Assert.AreEqual(org.RotationDays[10].RestrictionCollection.Count,
                            loadedAggregateFromDatabase.RotationDays[10].RestrictionCollection.Count);
            Assert.AreEqual(org.RotationDays[10].RestrictionCollection[0].WorkTimeLimitation,
                            loadedAggregateFromDatabase.RotationDays[10].RestrictionCollection[0].
                                WorkTimeLimitation);

            Assert.AreEqual(org.RotationDays[10].RestrictionCollection[0].StartTimeLimitation.StartTime,
                loadedAggregateFromDatabase.RotationDays[10].RestrictionCollection[0].StartTimeLimitation.StartTime);

            Assert.AreEqual(org.RotationDays[10].RestrictionCollection[0].StartTimeLimitation.EndTime,
                loadedAggregateFromDatabase.RotationDays[10].RestrictionCollection[0].StartTimeLimitation.EndTime);


        }

        protected override Repository<IRotation> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return RotationRepository.DONT_USE_CTOR(currentUnitOfWork.Current());
        }


        [Test]
        public void VerifyLoadAllWithRotationAndRotationDaysAndRestrictions()
        {
            IRotation rotation = CreateAggregateWithCorrectBusinessUnit();
            WorkTimeLimitation workTimeLimitation = new WorkTimeLimitation(new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0));
            rotation.FindRotationDay(7).RestrictionCollection[0].ShiftCategory = _shiftCategory;
            rotation.FindRotationDay(7).RestrictionCollection[0].DayOffTemplate = _dayOff;
            rotation.FindRotationDay(10).RestrictionCollection[0].WorkTimeLimitation = workTimeLimitation;
            rotation.FindRotationDay(11).RestrictionCollection[0].WorkTimeLimitation = workTimeLimitation;

            PersistAndRemoveFromUnitOfWork(rotation);

            IList<IRotation> res = new List<IRotation>(RotationRepository.DONT_USE_CTOR(UnitOfWork).LoadAllRotationsWithHierarchyData());

            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(21, res[0].RotationDays.Count);
            Assert.AreEqual(_shiftCategory, res[0].FindRotationDay(7).RestrictionCollection[0].ShiftCategory);
            Assert.AreEqual(_dayOff, res[0].FindRotationDay(7).RestrictionCollection[0].DayOffTemplate);
            Assert.AreEqual(new TimeSpan(7, 0, 0), res[0].FindRotationDay(10).RestrictionCollection[0].WorkTimeLimitation.StartTime);
            Assert.AreEqual(new TimeSpan(9, 0, 0), res[0].FindRotationDay(11).RestrictionCollection[0].WorkTimeLimitation.EndTime);
        }

        [Test]
        public void VerifyLoadAllWithRotationAndRotationDays()
        {
            IRotation rotation = CreateAggregateWithCorrectBusinessUnit();
            WorkTimeLimitation workTimeLimitation = new WorkTimeLimitation(new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0));
            rotation.FindRotationDay(7).RestrictionCollection[0].ShiftCategory = _shiftCategory;
            rotation.FindRotationDay(7).RestrictionCollection[0].DayOffTemplate = _dayOff;
            rotation.FindRotationDay(10).RestrictionCollection[0].WorkTimeLimitation = workTimeLimitation;
            rotation.FindRotationDay(11).RestrictionCollection[0].WorkTimeLimitation = workTimeLimitation;

            PersistAndRemoveFromUnitOfWork(rotation);

            IList<IRotation> res = new List<IRotation>(RotationRepository.DONT_USE_CTOR(UnitOfWork).LoadAllRotationsWithDays());

            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(21, res[0].RotationDays.Count);
			Assert.IsFalse(LazyLoadingManager.IsInitialized(res[0].FindRotationDay(7).RestrictionCollection));
        }

        [Test]
        public void VerifyPersistWhenDaysAreRemoved()
        {
            IRotation rotation = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(rotation);   // Persist instance (OK).
            rotation = Session.Get<Rotation>(rotation.Id);
            Assert.AreEqual(21, rotation.DaysCount);
            rotation.RemoveDays(7);                     // Removes few days from rotation.
            PersistAndRemoveFromUnitOfWork(rotation);
            Assert.AreEqual(14, Session.Get<Rotation>(rotation.Id).DaysCount);
        }

        [Test]
        public void ShouldLoadAggregateFromId()
        {
            var aggregate = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(aggregate);

            var loaded = RotationRepository.DONT_USE_CTOR(UnitOfWork).LoadAggregate(aggregate.Id.Value);
            Assert.IsNotNull(loaded);
        }
    }
}