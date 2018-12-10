using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;



namespace Teleopti.Ccc.DomainTest.Collection
{
	[DomainTest]
	public class CachedNumberOfEachCategoryPerDateTest
	{
		private ICachedNumberOfEachCategoryPerDate _target;
		private ScheduleDictionaryForTest _dic;
	    private IPerson _person;
	    private IShiftCategory _shiftCategory;
		private Scenario _scenario;

		[SetUp]
		public void Setup()
		{
			_scenario = ScenarioFactory.CreateScenario("Default", true, true);
			_dic = new ScheduleDictionaryForTest(_scenario, new DateTimePeriod(2013, 09, 12, 2013, 09, 13));
	        _person = PersonFactory.CreatePerson();
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej"); 
		}

		[Test]
		public void ShouldReturnValueForKey()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson().WithId();
			var person2 = PersonFactory.CreatePerson().WithId();
			var personList = new List<IPerson> {person1, person2};

			_dic.AddPersonAssignment(
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, _scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 17), _shiftCategory));

			_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
			_target.SetFilteredPersons(personList);
			IDictionary<IShiftCategory, int> value = _target.GetValue(new DateOnly(2013, 09, 12));
			Assert.AreEqual(1, value[_shiftCategory]);
			//and if we call it again with the same date the key will be found
			value = _target.GetValue(new DateOnly(2013, 09, 12));
			Assert.AreEqual(1, value[_shiftCategory]);
		}

		[Test]
		public void SettingFilteredPersonsShouldClearTheCache()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> {person1};

			_dic.AddPersonAssignment(
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, _scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 17), _shiftCategory));

			_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
			_target.SetFilteredPersons(personList);
			_target.GetValue(new DateOnly(2013, 09, 12));
			_target.GetValue(new DateOnly(2013, 09, 13));
			Assert.AreEqual(2, _target.ItemCount);
			_target.SetFilteredPersons(personList);
			Assert.AreEqual(0, _target.ItemCount);
		}

		[Test]
		public void ShouldClearKeyIfScheduleForPersonWithinMonitoringPeriodChanges()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> {person1};

			_dic.AddPersonAssignment(
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, _scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 17), _shiftCategory));

			_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
			_target.SetFilteredPersons(personList);
			_target.GetValue(new DateOnly(2013, 09, 12));
			_target.GetValue(new DateOnly(2013, 09, 13));
			Assert.AreEqual(2, _target.ItemCount);
			//fire event for a change on one day

			_dic.Modify(ScheduleModifier.Scheduler, _dic[person1].ScheduledDay(periodToMonitor.StartDate), NewBusinessRuleCollection.Minimum(),
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());
			
			Assert.AreEqual(1, _target.ItemCount);
		}

		[Test]
		public void ShouldClearAllIfUnknownChanges()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 13);
			var person1 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> {person1};

			_dic.AddPersonAssignment(
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, _scenario, ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2013, 09, 12, 8, 2013, 09, 12, 17), _shiftCategory));

			_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
			_target.SetFilteredPersons(personList);
			_target.GetValue(new DateOnly(2013, 09, 12));
			_target.GetValue(new DateOnly(2013, 09, 13));
			Assert.AreEqual(2, _target.ItemCount);
			//fire event for a change on one day

			_dic.Modify(ScheduleModifier.Scheduler, _dic[person1].ScheduledDay(periodToMonitor.StartDate), NewBusinessRuleCollection.Minimum(),
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());
		}

		[Test]
		public void ShouldNotConsiderPersonWhoHaveLeft()
		{
			var periodToMonitor = new DateOnlyPeriod(2013, 09, 12, 2013, 09, 16);
			var dateToMonitor = new DateOnly(2013, 09, 13);
			var personList = new List<IPerson> {_person};

			_person.TerminatePerson(dateToMonitor, new PersonAccountUpdaterDummy());

			_target = new CachedNumberOfEachCategoryPerDate(_dic, periodToMonitor);
			_target.SetFilteredPersons(personList);
			var result = _target.GetValue(dateToMonitor.AddDays(1));
			Assert.AreEqual(0, result.Count);
		}
	}
}