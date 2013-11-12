using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UndoRedo;
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
        private IScheduleDictionarySaver _target;

        protected override void SetupForRepositoryTest()
        {
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

            IScheduleDictionaryPersisterResult result = _target.MarkForPersist(UnitOfWork, new ScheduleRepository(UnitOfWork), diffColl);

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

            _target.MarkForPersist(UnitOfWork, new ScheduleRepository(UnitOfWork), diffColl);
            
            Assert.IsNull(Session.Get<PersonAssignment>(schedData.Id));
        }

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
	        schedDataModified.AddMainLayer(act, schedData.Period);
					schedDataModified.SetShiftCategory(cat);
            
            diffColl.Add(new DifferenceCollectionItem<IPersistableScheduleData>(schedData, schedDataModified));

			_target.MarkForPersist(UnitOfWork, new ScheduleRepository(UnitOfWork), diffColl);

            Assert.AreEqual(cat.Id.GetValueOrDefault(), Session.Get<PersonAssignment>(schedData.Id).ShiftCategory.Id.GetValueOrDefault());
        }

        [Test]
        public void VerifyAddScheduleDictionaryWhenNewAfterUndoRedo()
        {
            var person = PersonFactory.CreatePerson();
            var scenario = new Scenario("sdd");
            var cat = new ShiftCategory("ballefjong");

            PersistAndRemoveFromUnitOfWork(scenario);
            PersistAndRemoveFromUnitOfWork(person);
            PersistAndRemoveFromUnitOfWork(cat);

            var date = new DateOnly(2000, 1, 1);
            var schedData = new PersonAssignment(person, scenario, date);
            var preferenceData = new PreferenceDay(person, date, new PreferenceRestriction{ShiftCategory = cat});

            PersistAndRemoveFromUnitOfWork(preferenceData);
            PersistAndRemoveFromUnitOfWork(schedData);

            var scheduleRepository = new ScheduleRepository(UnitOfWork);
            var persons = new[] {person};
            var dictionary = scheduleRepository.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateOnlyPeriod(date,date).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone())), 
                                                              scenario, new PersonProvider(persons), new ScheduleDictionaryLoadOptions(true,false), persons);

            var undoRedo = new UndoRedoContainer(10);
            undoRedo.CreateBatch("preference");

            var day = dictionary[person].ScheduledDay(date);
            day.Add(new PreferenceDay(person, date, new PreferenceRestriction { ShiftCategory = cat }));
            dictionary.Modify(ScheduleModifier.Scheduler, new []{day}, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));

            undoRedo.CommitBatch();

            var diffColl = dictionary.DifferenceSinceSnapshot();
            _target.MarkForPersist(UnitOfWork, scheduleRepository, diffColl);
           
            undoRedo.CreateBatch("preference");

            day = dictionary[person].ScheduledDay(date);
            day.Add(new PreferenceDay(person, date, new PreferenceRestriction { ShiftCategory = cat }));
            dictionary.Modify(ScheduleModifier.Scheduler, new[] { day }, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));

            undoRedo.CommitBatch();

            undoRedo.CreateBatch("preference");

            day = dictionary[person].ScheduledDay(date);
            day.Add(new PreferenceDay(person, date, new PreferenceRestriction { ShiftCategory = cat }));
            dictionary.Modify(ScheduleModifier.Scheduler, new[] { day }, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));

            undoRedo.CommitBatch();

            undoRedo.Undo();

            diffColl = dictionary.DifferenceSinceSnapshot();
            _target.MarkForPersist(UnitOfWork, scheduleRepository, diffColl);
            
            Assert.IsNotNull(Session.Get<PersonAssignment>(schedData.Id));
        }
    }
}
