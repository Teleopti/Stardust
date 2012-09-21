using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class TeamSteadyStateDictionaryCreatorTest
	{
		private TeamSteadyStateDictionaryCreator _target;
		private MockRepository _mocks;
		private IGroupPerson _groupPerson1;
		private IGroupPerson _groupPerson2;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private IPerson _person4;
		private IList<IGroupPerson> _groupPersons;
		private ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleMatrixPro _scheduleMatrixPro3;
		private IScheduleMatrixPro _scheduleMatrixPro4;
		private IList<IScheduleMatrixPro> _matrixes;
		private IRuleSetBag _ruleSetBag;
		private IWorkShiftRuleSet _workShiftRuleSet;
		private IWorkShiftTemplateGenerator _workShiftTemplateGenerator;
		private IShiftCategory _shiftCategory;
		
		private DateOnly _dateOnly;

		private ISchedulingResultStateHolder _stateHolder;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();

			_dateOnly = new DateOnly(2012, 1, 1);
			var dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(30));

			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person1"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person2"), _dateOnly);
			_person3 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person3"), _dateOnly);
			_person4 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person4"), _dateOnly);

			_shiftCategory = new ShiftCategory("shiftCategory");
			_ruleSetBag = new RuleSetBag();
			_workShiftTemplateGenerator = new WorkShiftTemplateGenerator(new Activity("activity"), new TimePeriodWithSegment(), new TimePeriodWithSegment(), _shiftCategory );
			_workShiftRuleSet = new WorkShiftRuleSet(_workShiftTemplateGenerator);
			_ruleSetBag.AddRuleSet(_workShiftRuleSet);

			_person1.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))[0].RuleSetBag = _ruleSetBag;
			_person2.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))[0].RuleSetBag = _ruleSetBag;
			_person3.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))[0].RuleSetBag = _ruleSetBag;
			_person4.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))[0].RuleSetBag = _ruleSetBag;

			_groupPerson1 = new GroupPerson(new List<IPerson>{_person1, _person2}, _dateOnly, "groupPerson1");
			_groupPerson2 = new GroupPerson(new List<IPerson> { _person3, _person4 }, _dateOnly, "groupPerson2");
			_groupPersons = new List<IGroupPerson> { _groupPerson1, _groupPerson2 };

			SetupStateHolder();

			_scheduleMatrixPro1 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person1, _person1.VirtualSchedulePeriod(_dateOnly));
			_scheduleMatrixPro2 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person2, _person2.VirtualSchedulePeriod(_dateOnly));
			_scheduleMatrixPro3 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person3, _person3.VirtualSchedulePeriod(_dateOnly));
			_scheduleMatrixPro4 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person4, _person4.VirtualSchedulePeriod(_dateOnly));

			_schedulePeriodTargetTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
			_matrixes = new List<IScheduleMatrixPro>{_scheduleMatrixPro1, _scheduleMatrixPro2, _scheduleMatrixPro3, _scheduleMatrixPro4};
			
			_target = new TeamSteadyStateDictionaryCreator(_groupPersons, _schedulePeriodTargetTimeCalculator, _matrixes );	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void SetupStateHolder()
		{
			var wholePeriod = new DateTimePeriod(2012, 1, 1, 2012, 1, 31);
			IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(wholePeriod);
			IScenario scenario = new Scenario("Scenario");
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>());

			var dayPeriod = new DateTimePeriod(2012, 1, 1, 2012, 1, 31);
			IScheduleParameters parameters1 = new ScheduleParameters(scenario, _person1, dayPeriod);
			IScheduleParameters parameters2 = new ScheduleParameters(scenario, _person2, dayPeriod);
			IScheduleParameters parameters3 = new ScheduleParameters(scenario, _person3, dayPeriod);
			IScheduleParameters parameters4 = new ScheduleParameters(scenario, _person4, dayPeriod);
			IScheduleRange range1 = new ScheduleRange(scheduleDictionary, parameters1);
			IScheduleRange range2 = new ScheduleRange(scheduleDictionary, parameters2);
			IScheduleRange range3 = new ScheduleRange(scheduleDictionary, parameters3);
			IScheduleRange range4 = new ScheduleRange(scheduleDictionary, parameters4);

			scheduleDictionary.AddTestItem(_person1, range1);
			scheduleDictionary.AddTestItem(_person2, range2);
			scheduleDictionary.AddTestItem(_person3, range3);
			scheduleDictionary.AddTestItem(_person4, range4);

			_stateHolder = new SchedulingResultStateHolder { Schedules = scheduleDictionary };
		}

		[Test]
		public void ShouldCreateDictionary()
		{
			var timePeriod = new TimePeriod();

			using(_mocks.Record())
			{
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro1)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro2)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro3)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro4)).Return(timePeriod);
			}

			using(_mocks.Playback())
			{
				var result = _target.Create(_dateOnly);
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result["groupPerson1"]);
				Assert.IsTrue(result["groupPerson2"]);
			}		
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenPersonPeriodValueDiffers()
		{
			var timePeriod = new TimePeriod();
			var personPeriod = _person1.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))[0];
			var partTimePercentage = new PartTimePercentage("partTimePercentage");
			partTimePercentage.SetId(Guid.NewGuid());
			personPeriod.PersonContract.PartTimePercentage = partTimePercentage;

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro1)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro2)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro3)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro4)).Return(timePeriod);
			}

			using (_mocks.Playback())
			{
				var result = _target.Create(_dateOnly);
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsFalse(result["groupPerson1"]);
				Assert.IsTrue(result["groupPerson2"]);
			}	
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenSchedulePeriodValueDiffers()
		{
			var timePeriod1 = new TimePeriod();
			var timePeriod2 = new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(2));

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro1)).Return(timePeriod1);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro2)).Return(timePeriod1);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro3)).Return(timePeriod1);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro4)).Return(timePeriod2);
			}

			using (_mocks.Playback())
			{
				var result = _target.Create(_dateOnly);
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result["groupPerson1"]);
				Assert.IsFalse(result["groupPerson2"]);
			}		
		}
	}
}
