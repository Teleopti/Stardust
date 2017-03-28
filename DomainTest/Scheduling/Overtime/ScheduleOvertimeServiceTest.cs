using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[DomainTest]
	[TestWithStaticDependenciesAvoidUse]
	public class ScheduleOvertimeServiceTest
	{
		public IScheduleOvertimeService Target;
		public IResourceCalculation ResourceOptimizationHelper;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeTimeZoneGuard TimeZoneGuard;

		private ScheduleOvertimeService _target;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay;
		private IOvertimePreferences _overtimePreferences;
		private IOvertimeLengthDecider _overtimeLengthDecider;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private DateOnly _dateOnly;
		private IScheduleTagSetter _scheduleTagSetter;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private ISkill _skill;
		private DateTimePeriod _dateTimePeriod;
		private IActivity _activity;
		private ISkillStaffPeriodHolder _skillStaffPeriodHolder;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_overtimePreferences = new OvertimePreferences();
			_overtimeLengthDecider = _mock.StrictMock<IOvertimeLengthDecider>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_dateOnly = new DateOnly(2014, 1, 1);
			_scheduleTagSetter = _mock.StrictMock<IScheduleTagSetter>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_target = new ScheduleOvertimeService(_overtimeLengthDecider, _schedulePartModifyAndRollbackService, _schedulingResultStateHolder, new GridlockManager(), new FakeTimeZoneGuard(), new PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider(new PersonSkillsUseAllForScheduleDaysOvertimeProvider(), new PersonalSkillsProvider()));
			_person = PersonFactory.CreatePerson("person");
			_activity = ActivityFactory.CreateActivity("activity");
			_skill = SkillFactory.CreateSkill("skill");
			_skill.Activity = _activity;
			_personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkills(_dateOnly, new[] {_skill});
			_person.AddPersonPeriod(_personPeriod);
			_dateTimePeriod = new DateTimePeriod(2014, 1, 1, 2014, 1, 2);
			_overtimePreferences.SkillActivity = _activity;
			_overtimePreferences.SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0));
			_skillStaffPeriodHolder = _mock.StrictMock<ISkillStaffPeriodHolder>();
			_multiplicatorDefinitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			_personPeriod.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet);
		}

		[Test]
		public void ShouldSchedulePersonOnDay()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			skill.TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var dateOnly = new DateOnly(2016, 7, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var shiftCategory = new ShiftCategory("_").WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromHours(10));
			SkillDayRepository.Has(new List<ISkillDay> { skillDay });
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly, new TimePeriod(10, 0, 11, 0));
			var ass = PersonAssignmentRepository.GetSingle(dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), new[] { agent }, new[] { ass }, new[] { skillDay });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0)),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = phoneActivity
			};
			var resourceCalculateDelayer = new ResourceCalculateDelayer(ResourceOptimizationHelper, 1, true, stateHolder.SchedulingResultState, UserTimeZone.Make());
			TimeZoneGuard.SetTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var scheduleTagSetter = new ScheduleTagSetter(overtimePreference.ScheduleTag);
			Target.SchedulePersonOnDay(stateHolder.Schedules[agent].ScheduledDay(dateOnly), overtimePreference,
				resourceCalculateDelayer, dateOnly, scheduleTagSetter);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldSkipWhenNoSuitablePeriods()
		{
			var period = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 10);
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), new ServiceAgreement());
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), new ServiceAgreement());
			skillStaffPeriod2.SetCalculatedResource65(10);
			IList<ISkillStaffPeriod> skillStaffPeriods1 = new List<ISkillStaffPeriod> { skillStaffPeriod1 };

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_overtimeLengthDecider.Decide(new OvertimePreferences(), _person, _dateOnly, _scheduleDay,
					new MinMax<TimeSpan>(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0)), new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.Zero),
					false)).IgnoreArguments().Return(new List<DateTimePeriod>());
				Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skill, _dateTimePeriod)).Return(skillStaffPeriods1).Repeat.AtLeastOnce().IgnoreArguments();
			}

			using (_mock.Playback())
			{
				var res = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _scheduleTagSetter);
				Assert.IsFalse(res);
			}	
		}

		[Test]
		public void ShouldSkipWhenNoMultiplicatorDefinitionSetOfTypeOvertime()
		{
			_personPeriod.PersonContract.Contract.RemoveMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
			}

			using (_mock.Playback())
			{
				var res = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _scheduleTagSetter);
				Assert.IsFalse(res);
			}
		}

		[Test]
		public void ShouldSkipWhenNoPersonPeriod()
		{
			_person.RemoveAllPersonPeriods();

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
			}

			using (_mock.Playback())
			{
				var res = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _scheduleTagSetter);
				Assert.IsFalse(res);
			}	
		}

		[Test]
		public void ShouldSkipWhenNoPersonContract()
		{
			_person.PersonPeriodCollection[0].PersonContract = null;

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
			}

			using (_mock.Playback())
			{
				var result = _target.SchedulePersonOnDay(_scheduleDay, _overtimePreferences, _resourceCalculateDelayer, _dateOnly, _scheduleTagSetter);
				Assert.IsFalse(result);
			}		
		}

		[Test]
		public void ShouldOnlyScheduleOverTimeOnSelectedTimePeriodInViewPointTimeZoneBug39740()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			var skill = SkillRepository.Has("skill", phoneActivity);
			skill.TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var dateOnly = new DateOnly(2016, 7, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, skill);
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.TaipeiTimeZoneInfo());
			var shiftCategory = new ShiftCategory("_").WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromHours(10));
			var skillDay1 = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), TimeSpan.FromHours(10));
			var skillDay2 = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(2), TimeSpan.FromHours(10));
			SkillDayRepository.Has(new List<ISkillDay> { skillDay, skillDay1, skillDay2 });
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, dateOnly.AddDays(1), new TimePeriod(10, 0, 11, 0));
			var ass = PersonAssignmentRepository.GetSingle(dateOnly.AddDays(1));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), new[] { agent }, new[] { ass }, new[] { skillDay, skillDay1, skillDay2 });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0)),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = phoneActivity
			};
			var resourceCalculateDelayer = new ResourceCalculateDelayer(ResourceOptimizationHelper, 1, true, stateHolder.SchedulingResultState, UserTimeZone.Make());
			var rules = NewBusinessRuleCollection.Minimum();
			var scheduleTagSetter = new ScheduleTagSetter(overtimePreference.ScheduleTag);
			TimeZoneGuard.SetTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
			Target.SchedulePersonOnDay(stateHolder.Schedules[agent].ScheduledDay(dateOnly.AddDays(1)), overtimePreference,
				resourceCalculateDelayer, dateOnly.AddDays(1), scheduleTagSetter);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly.AddDays(1)).PersonAssignment(true).OvertimeActivities().Should().Be.Empty();
		}
	}
}
