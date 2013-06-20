﻿using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
    //the old test class I cannot follow anymore. 
    //mock hell has occured since I saw it last time...
    //need to use something from scratch
    [TestFixture]
    [Category("LongRunning")]
    public class ScheduleDictionaryPersisterIntegrationTest : DatabaseTest
    {
        private MockRepository _mocks;
        private IScheduleDictionarySaver _target;

        protected override void SetupForRepositoryTest()
        {
            _mocks = new MockRepository();
            _target = new ScheduleDictionarySaver();
        }

        [Test]
        public void VerifyAddScheduleDictionaryWhenAdd()
        {
            var diffColl = new DifferenceCollection<IPersistableScheduleData>();
						var schedData = new PersonAssignment(PersonFactory.CreatePerson(), new Scenario("sdd"), new DateOnly(2000, 1, 1));
            PersistAndRemoveFromUnitOfWork(schedData.Scenario);
            PersistAndRemoveFromUnitOfWork(schedData.Person);
            diffColl.Add(new DifferenceCollectionItem<IPersistableScheduleData>(null, schedData));

            IScheduleDictionaryPersisterResult result;
            using (_mocks.Playback())
            {
				result = _target.MarkForPersist(UnitOfWork, new ScheduleRepository(UnitOfWork), diffColl);
            }

            Assert.IsNotNull(Session.Get<PersonAssignment>(result.AddedEntities.Single().Id));
        }


        [Test]
        public void VerifyAddScheduleDictionaryWhenDelete()
        {
            var diffColl = new DifferenceCollection<IPersistableScheduleData>();
						var schedData = new PersonAssignment(PersonFactory.CreatePerson(), new Scenario("sdd"), new DateOnly(2000, 1, 1));
            PersistAndRemoveFromUnitOfWork(schedData.Scenario);
            PersistAndRemoveFromUnitOfWork(schedData.Person);
            PersistAndRemoveFromUnitOfWork(schedData);
            diffColl.Add(new DifferenceCollectionItem<IPersistableScheduleData>(schedData, null));

            using (_mocks.Playback())
            {
				_target.MarkForPersist(UnitOfWork, new ScheduleRepository(UnitOfWork), diffColl);
            }

            Assert.IsNull(Session.Get<PersonAssignment>(schedData.Id));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        public void VerifyAddScheduleDictionaryWhenChanged()
        {
            var diffColl = new DifferenceCollection<IPersistableScheduleData>();
						var schedData = new PersonAssignment(PersonFactory.CreatePerson(), new Scenario("sdd"), new DateOnly(2000, 1, 1));
            var cat = new ShiftCategory("ballefjong");
	        var act = new Activity("sdfasdfa");
            PersistAndRemoveFromUnitOfWork(schedData.Scenario);
            PersistAndRemoveFromUnitOfWork(schedData.Person);
            PersistAndRemoveFromUnitOfWork(schedData);
            PersistAndRemoveFromUnitOfWork(cat);
					PersistAndRemoveFromUnitOfWork(act);

            var schedDataModified = schedData.EntityClone();
						schedDataModified.SetMainShiftLayers(new[] { new MainShiftLayer(act, schedData.Period) }, cat);
            
            diffColl.Add(new DifferenceCollectionItem<IPersistableScheduleData>(schedData, schedDataModified));



			_target.MarkForPersist(UnitOfWork, new ScheduleRepository(UnitOfWork), diffColl);

            Assert.AreEqual(cat.Id.Value, Session.Get<PersonAssignment>(schedData.Id).ShiftCategory.Id.Value);
        }
    }
}
