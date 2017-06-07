using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[DomainTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx),
		Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388),
		Toggle(Toggles.Staffing_ReadModel_BetterAccuracy_Step3_44331)]
	public class ScheduleOvertimeExecuteWrapperTest : ISetup
	{
		public ScheduleOvertimeExecuteWrapper Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;

		private IScenario scenario;
		private IActivity activity;
		private ISkill skill;
		private IMultiplicatorDefinitionSet multiplicatorDefinitionSet;
		private IShiftCategory shiftCategory;
		private IContract contract;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ScheduleOvertimeExecuteWrapper>().For<ScheduleOvertimeExecuteWrapper>();
			system.UseTestDouble<ScheduleOvertimeServiceWithoutStateholder>().For<IScheduleOvertimeServiceWithoutStateholder>();
			system.UseTestDouble<ScheduleOvertimeWithoutStateHolder>().For<ScheduleOvertimeWithoutStateHolder>();
			system.UseTestDouble<GridlockManager>().For<IGridlockManager>();
		}

		private void setup()
		{
			scenario = ScenarioRepository.Has("scenario");
			activity = ActivityRepository.Has("activity");
			skill = SkillRepository.Has("skill", activity);
			skill.DefaultResolution = 60;
			shiftCategory = new ShiftCategory("shiftCategory").WithId();
			multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			contract = new Contract("contract");
			contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);

			Now.Is("2017-06-01 08:00");
		}

		[Test]
		public void ShouldScheduleOvertimeOnAgent()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 15, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 16, 0, 0).Utc(),
					Resource = 1,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};

			var dateTimePeriod = dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] {agent}), new ScheduleDictionaryLoadOptions(false, false), new[] {agent});
			var affectedPersons = Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { scheduleDictionary[agent].ScheduledDay(dateOnly)}, dateTimePeriod, new[] {skill});
			var overtimeActivities = scheduleDictionary[agent].ScheduledDay(dateOnly).PersonAssignment().OvertimeActivities().Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 16, 2017, 06, 01, 17));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			affectedPersons.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldScheduleOvertimeOnOnlyOneAgentWhenMoreAreAvailable()
		{
			setup();
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 1, 15, 0, 0).Utc(),
					EndDateTime = new DateTime(2017, 06, 01, 16, 0, 0).Utc(),
					Resource = 2,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			var dateOnly = new DateOnly(2017, 06, 1);

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass2 = new PersonAssignment(agent2, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(15, 16));
			PersonAssignmentRepository.Has(ass2);

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, dateOnly, 1));

			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = multiplicatorDefinitionSet,
				ScheduleTag = new NullScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};

			var dateTimePeriod = dateOnly.ToDateTimePeriod(TimeZoneInfo.Utc);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod), scenario, new PersonProvider(new[] {agent, agent2}), new ScheduleDictionaryLoadOptions(false, false), new[] {agent, agent2});
			var scheduleDays = scheduleDictionary.SchedulesForDay(dateOnly).ToList();
			var affectedPersons = Target.Execute(overtimePreference, new NoSchedulingProgress(), scheduleDays, dateTimePeriod, new[] {skill});
			var overtimeActivities = scheduleDictionary.SchedulesForDay(dateOnly).ToList()
				.Select(x => x.PersonAssignment().OvertimeActivities())
				.SelectMany(i => i).Where(ot => ot.Period == new DateTimePeriod(2017, 06, 01, 16, 2017, 06, 01, 17));
			overtimeActivities.Count().Should().Be.EqualTo(1);
			affectedPersons.Count.Should().Be.EqualTo(1);
		}



		//[Test]
		//public void ShouldHandleCasesWhereSkillsTimeZoneIsFarAway()
		//{
		//	TimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDays = new List<ISkillDay>
		//	{
		//		skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(-1), 10),
		//		skill.CreateSkillDayWithDemand(scenario, dateOnly, 10),
		//		skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 10)
		//	};
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(1, 2));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDays);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(0, 0, 30, 0),
		//		SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldPlaceOvertimeWhenViewerAndAgentTimeZonesAreFarAway([Values("W. Europe Standard Time", "Mountain Standard Time")] string viewersTimeZone)
		//{
		//	TimeZoneGuard.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(viewersTimeZone));
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agentUserTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
		//	var agent = new Person().WithId().InTimeZone(agentUserTimeZone).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDays = new List<ISkillDay>
		//	{
		//		skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(-1), TimeSpan.FromMinutes(60)),
		//		skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15))
		//	};
		//	var ass = new PersonAssignment(agent, scenario, dateOnly);
		//	ass.AddActivity(activity, new DateTimePeriod(2015, 10, 11, 23, 2015, 10, 12, 8));
		//	ass.SetShiftCategory(shiftCategory);
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDays);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(0, 0, 30, 0),
		//		SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldScheduleNextAvailablePeriodIfCurrentCouldNotBeScheduled()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_") { InWorkTime = true };
		//	var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
		//	var assNextDay = new PersonAssignment(agent, scenario, dateOnly.AddDays(1)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass, assNextDay }, skillDay);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(18, 0, 20, 0),
		//		SelectedTimePeriod = new TimePeriod(1, 0, 1, 15),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly)
		//		.PersonAssignment(true)
		//		.OvertimeActivities()
		//		.Should()
		//		.Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldConsiderAllPeriodsWhenAvailableAgentsOnly()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_") { InWorkTime = true };
		//	var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
		//	var overtimeAvailability = new OvertimeAvailability(agent, dateOnly, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
		//	var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
		//	scheduleDay.Add(overtimeAvailability);
		//	scheduleDay.ModifyDictionary();
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(0, 0, 20, 0),
		//		SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
		//		SkillActivity = activity,
		//		AvailableAgentsOnly = true
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly)
		//		.PersonAssignment(true)
		//		.OvertimeActivities()
		//		.Should()
		//		.Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldConsiderOvertimePreferenceMinimumOvertimeLengthBug41951()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_") { InWorkTime = true };
		//	var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
		//	var overtimeAvailability = new OvertimeAvailability(agent, dateOnly, new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
		//	var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
		//	scheduleDay.Add(overtimeAvailability);
		//	scheduleDay.ModifyDictionary();
		//	//agent applied for 30 minutes, minimum length is 60 minutes
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(0, 0, 20, 0),
		//		SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
		//		SkillActivity = activity,
		//		AvailableAgentsOnly = true
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly)
		//		.PersonAssignment(true)
		//		.OvertimeActivities()
		//		.Should().Be.Empty();
		//}

		//[Test]
		//public void ShouldScheduleNextAvailablePeriodIfCurrentIsNotBetter()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_") { InWorkTime = true };
		//	var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 0.1, ServiceAgreement.DefaultValues(), new Tuple<TimePeriod, double>(new TimePeriod(9, 45, 10, 0), 1));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(0, 0, 10, 0),
		//		SelectedTimePeriod = new TimePeriod(0, 30, 0, 45),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldAddOvertimeOnPartOfSkillInterval()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
		//		SelectedTimePeriod = new TimePeriod(0, 30, 0, 30),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldAddOvertimeOnShiftNotEndingOnFullHour()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 0, 16, 5));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
		//		SelectedTimePeriod = new TimePeriod(0, 60, 0, 60),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldAddOvertimeOnShiftNotStartingOnFullHour()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 5, 16, 0));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(6, 0, 8, 5),
		//		SelectedTimePeriod = new TimePeriod(0, 60, 0, 60),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldUseMinimumResolutionThatFitsMinDuration()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 0, 16, 45));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
		//		SelectedTimePeriod = new TimePeriod(0, 60, 0, 60),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		//}

		//[Test]
		//public void ShouldAdjustToMappedDataEnd()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpenBetween(7, 17);
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay1 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var skillDay2 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 0, 16, 8));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, new[] { skillDay1, skillDay2 });
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
		//		SelectedTimePeriod = new TimePeriod(0, 45, 0, 60),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	var openHours = skillDay1.OpenHours().First();
		//	var overtimePeriod = stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().First().Period.TimePeriod(TimeZoneInfo.Utc);
		//	openHours.EndTime.Should().Be.EqualTo(overtimePeriod.EndTime);
		//}

		//[Test]
		//public void ShouldAdjustToMappedDataStart()
		//{
		//	var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
		//	var activity = new Activity("_");
		//	var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpenBetween(8, 17);
		//	var dateOnly = new DateOnly(2015, 10, 12);
		//	var scenario = new Scenario("_");
		//	var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
		//	var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
		//	contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
		//	var shiftCategory = new ShiftCategory("_").WithId();
		//	var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
		//	var skillDay1 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var skillDay2 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
		//	var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 58, 16, 0));
		//	var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, new[] { skillDay1, skillDay2 });
		//	var overtimePreference = new OvertimePreferences
		//	{
		//		OvertimeType = definitionSet,
		//		ScheduleTag = new ScheduleTag(),
		//		SelectedSpecificTimePeriod = new TimePeriod(6, 0, 10, 0),
		//		SelectedTimePeriod = new TimePeriod(0, 45, 0, 60),
		//		SkillActivity = activity
		//	};

		//	Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

		//	var openHours = skillDay1.OpenHours().First();
		//	var overtimePeriod = stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().First().Period.TimePeriod(TimeZoneInfo.Utc);
		//	openHours.StartTime.Should().Be.EqualTo(overtimePeriod.StartTime);
		//}


	}
}
