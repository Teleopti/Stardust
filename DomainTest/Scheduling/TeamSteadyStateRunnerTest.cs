using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateRunnerTest
	{
		private MockRepository _mocks;
		private TeamSteadyStateRunner _target;
		private IGroupPerson _groupPerson1;
		private IPerson _person1;
		private IPerson _person2;
		private DateOnly _dateOnly;
		private ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private ISchedulingResultStateHolder _stateHolder;
		private IRuleSetBag _ruleSetBag;
		private IShiftCategory _shiftCategory;
		private IWorkShiftTemplateGenerator _workShiftTemplateGenerator;
		private IWorkShiftRuleSet _workShiftRuleSet;
		private IList<IScheduleMatrixPro> _matrixes;
		private Guid _guid;

		[SetUp]
		public void Setup()
		{
			_guid = Guid.NewGuid();
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2012, 1, 1);
			var ctr = new Contract("ctr");
			var ptc = new PartTimePercentage("ptc");
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod	(PersonFactory.CreatePerson("person1"), _dateOnly, ctr, ptc);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person2"), _dateOnly, ctr, ptc);

			_shiftCategory = new ShiftCategory("shiftCategory");
			_ruleSetBag = new RuleSetBag();
			_workShiftTemplateGenerator = new WorkShiftTemplateGenerator(new Activity("activity"), new TimePeriodWithSegment(), new TimePeriodWithSegment(), _shiftCategory);
			_workShiftRuleSet = new WorkShiftRuleSet(_workShiftTemplateGenerator);
			_ruleSetBag.AddRuleSet(_workShiftRuleSet);

			_person1.Period(_dateOnly).RuleSetBag = _ruleSetBag;
			_person2.Period(_dateOnly).RuleSetBag = _ruleSetBag;
			_groupPerson1 = new GroupPerson(new List<IPerson> { _person1, _person2 }, _dateOnly, "groupPerson1", _guid);	
			var dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(30));
			
			SetupStateHolder();
			_scheduleMatrixPro1 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person1, _person1.VirtualSchedulePeriod(_dateOnly));
			_scheduleMatrixPro2 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person2, _person2.VirtualSchedulePeriod(_dateOnly));	

			_schedulePeriodTargetTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
			_matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2};

			_target = new TeamSteadyStateRunner(_matrixes, _schedulePeriodTargetTimeCalculator);	
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
			IScheduleRange range1 = new ScheduleRange(scheduleDictionary, parameters1);
			IScheduleRange range2 = new ScheduleRange(scheduleDictionary, parameters2);

			scheduleDictionary.AddTestItem(_person1, range1);
			scheduleDictionary.AddTestItem(_person2, range2);

			_stateHolder = new SchedulingResultStateHolder { Schedules = scheduleDictionary };
		}

		[Test]
		public void ShouldCreateReturnValue()
		{
			var timePeriod = new TimePeriod();

			using(_mocks.Record())
			{
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetTime(_scheduleMatrixPro1)).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetTime(_scheduleMatrixPro2)).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro1)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro2)).Return(timePeriod);	
			}

			using(_mocks.Playback())
			{
				var result = _target.Run(_groupPerson1, _dateOnly);
				Assert.IsNotNull(result);
				Assert.IsTrue(result.Value);
			}
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenPersonPeriodValueDiffers()
		{
			var timePeriod = new TimePeriod();
			var personPeriod = _person1.Period(_dateOnly);
			var partTimePercentage = new PartTimePercentage("partTimePercentage");
			partTimePercentage.SetId(Guid.NewGuid());
			personPeriod.PersonContract.PartTimePercentage = partTimePercentage;

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetTime(_scheduleMatrixPro1)).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetTime(_scheduleMatrixPro2)).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro1)).Return(timePeriod);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro2)).Return(timePeriod);
			}

			using (_mocks.Playback())
			{
				var result = _target.Run(_groupPerson1, _dateOnly);
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Value);
			}	
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenSchedulePeriodValueDiffers()
		{
			var timePeriod1 = new TimePeriod();
			var timePeriod2 = new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(2));

			using (_mocks.Record())
			{
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetTime(_scheduleMatrixPro1)).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetTime(_scheduleMatrixPro2)).Return(TimeSpan.FromHours(160));
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro1)).Return(timePeriod1);
				Expect.Call(_schedulePeriodTargetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro2)).Return(timePeriod2);
			}

			using (_mocks.Playback())
			{
				var result = _target.Run(_groupPerson1, _dateOnly);
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Value);
			}
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenNoPersonPeriodOnFirstPerson()
		{
			_person1.RemoveAllPersonPeriods();
			var result = _target.Run(_groupPerson1, _dateOnly);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Value);				
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenNoPersonPeriod()
		{
			_person2.RemoveAllPersonPeriods();

			var result = _target.Run(_groupPerson1, _dateOnly);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Value);	
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenNoScheduleMatrixProOnFirstPerson()
		{
			_matrixes.RemoveAt(0);
			var result = _target.Run(_groupPerson1, _dateOnly);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Value);
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenNoScheduleMatrixPro()
		{
			_matrixes.RemoveAt(1);
			var result = _target.Run(_groupPerson1, _dateOnly);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Value);
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenFirstPersonHasDifferentPersonPeriodsOnMatrix()
		{
			IPersonPeriod personPeriod = new PersonPeriod(_dateOnly.AddDays(10), PersonContractFactory.CreatePersonContract(), new Team());
			_person1.AddPersonPeriod(personPeriod);
			var result = _target.Run(_groupPerson1, _dateOnly);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Value);
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseWhenPersonHasDifferentPersonPeriodsOnMatrix()
		{
			IPersonPeriod personPeriod = new PersonPeriod(_dateOnly.AddDays(10), PersonContractFactory.CreatePersonContract(), new Team());
			_person2.AddPersonPeriod(personPeriod);
			var result = _target.Run(_groupPerson1, _dateOnly);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Value);
		}

		[Test]
		public void ShouldSetTeamSteadyStateToFalseOnDifferentMatrixPeriods()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(20));
			_matrixes.RemoveAt(1);
			_scheduleMatrixPro2 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person2, _person2.VirtualSchedulePeriod(_dateOnly));
			_matrixes.Add(_scheduleMatrixPro2);
			var result = _target.Run(_groupPerson1, _dateOnly);
			Assert.IsNotNull(result);
			Assert.IsFalse(result.Value);
		}
		
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionOnNullGroupPersonId()
		{
			_groupPerson1 = new GroupPerson(new List<IPerson> { _person1, _person2 }, _dateOnly, "groupPerson1", null);
			_target.Run(_groupPerson1, _dateOnly);
		}
	}
}
