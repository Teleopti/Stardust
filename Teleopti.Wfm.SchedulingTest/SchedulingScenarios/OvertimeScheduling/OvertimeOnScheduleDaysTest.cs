using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[DomainTest]
	public class OvertimeOnScheduleDaysTest : OvertimeSchedulingScenario
	{
		public ScheduleOvertime Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public IGridlockManager LockManager;
		public FakeTimeZoneGuard TimeZoneGuard;

		[Test]
		public void ShouldSkipLockedDaysWithAssignmentWithLayers()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};
			LockManager.AddLock(agent, dateOnly,LockType.Normal);

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });
			
			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleCasesWhereSkillsTimeZoneIsFarAway()
		{
			TimeZoneGuard.SetTimeZone(TimeZoneInfo.Utc);
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDays = new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(-1), 10),
				skill.CreateSkillDayWithDemand(scenario, dateOnly, 10),
				skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), 10)
			};
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(1, 2));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDays);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(0, 0, 30, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPlaceOvertimeWhenViewerAndAgentTimeZonesAreFarAway([Values("W. Europe Standard Time", "Mountain Standard Time")] string viewersTimeZone)
		{
			TimeZoneGuard.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(viewersTimeZone));
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).WithId().IsOpen().DefaultResolution(15);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agentUserTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
			var agent = new Person().WithId().InTimeZone(agentUserTimeZone).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDays = new List<ISkillDay>
			{
				skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(-1), TimeSpan.FromMinutes(60)),
				skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(15))
			};
			var ass = new PersonAssignment(agent, scenario, dateOnly);
			ass.AddActivity(activity, new DateTimePeriod(2015, 10, 11, 23, 2015, 10, 12, 8));
			ass.SetShiftCategory(shiftCategory);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDays);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(0, 0, 30, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldScheduleNextAvailablePeriodIfCurrentCouldNotBeScheduled()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_") {InWorkTime = true};
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
			var assNextDay = new PersonAssignment(agent, scenario, dateOnly.AddDays(1)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(6, 14));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass, assNextDay }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(18, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 15),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly)
				.PersonAssignment(true)
				.OvertimeActivities()
				.Should()
				.Not.Be.Empty();
		}

		[Test]
		public void ShouldConsiderAllPeriodsWhenAvailableAgentsOnly()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_") {InWorkTime = true};
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
			var otAvailability = new OvertimeAvailability(agent, dateOnly, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new IScheduleData[] {ass, otAvailability}, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(0, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity,
				AvailableAgentsOnly = true
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly)
				.PersonAssignment(true)
				.OvertimeActivities()
				.Should()
				.Not.Be.Empty();
		}

		[Test]
		public void ShouldConsiderOvertimePreferenceMinimumOvertimeLengthBug41951()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_") {InWorkTime = true};
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
			var overtimeAvailability = new OvertimeAvailability(agent, dateOnly, new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new IScheduleData[] { ass, overtimeAvailability }, skillDay);
			//agent applied for 30 minutes, minimum length is 60 minutes
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(0, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity,
				AvailableAgentsOnly = true
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly)
				.PersonAssignment(true)
				.OvertimeActivities()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldScheduleNextAvailablePeriodIfCurrentIsNotBetter()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_") {InWorkTime = true};
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen().DefaultResolution(15);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, dateOnly, 0.1, new Tuple<TimePeriod, double>(new TimePeriod(9, 45, 10, 0), 1));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 18));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(0, 0, 10, 0),
				SelectedTimePeriod = new TimePeriod(0, 30, 0, 45),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldAddOvertimeOnPartOfSkillInterval()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 17, 0),
				SelectedTimePeriod = new TimePeriod(0, 30, 0, 30),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldAddOvertimeOnShiftNotEndingOnFullHour()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 0, 16, 5));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(0, 60, 0, 60),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldAddOvertimeOnShiftNotStartingOnFullHour()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 5, 16, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(6, 0, 8, 5),
				SelectedTimePeriod = new TimePeriod(0, 60, 0, 60),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldUseMinimumResolutionThatFitsMinDuration()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 0, 16, 45));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(0, 60, 0, 60),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldAdjustToMappedDataEnd()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpenBetween(7, 17);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay1 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var skillDay2 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 0, 16, 8));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, new[] {skillDay1, skillDay2});
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(16, 0, 20, 0),
				SelectedTimePeriod = new TimePeriod(0, 45, 0, 60),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			var openHours = skillDay1.OpenHours().First();
			var overtimePeriod = stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().First().Period.TimePeriod(TimeZoneInfo.Utc);
			openHours.EndTime.Should().Be.EqualTo(overtimePeriod.EndTime);
		}

		[Test]
		public void ShouldAdjustToMappedDataStart()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpenBetween(8, 17);
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var skillDay1 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var skillDay2 = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 58, 16, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, new[] { ass }, new[] { skillDay1, skillDay2 });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(6, 0, 10, 0),
				SelectedTimePeriod = new TimePeriod(0, 45, 0, 60),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			var openHours = skillDay1.OpenHours().First();
			var overtimePeriod = stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().First().Period.TimePeriod(TimeZoneInfo.Utc);
			openHours.StartTime.Should().Be.EqualTo(overtimePeriod.StartTime);
		}

		[Test]
		public void ShouldHaveDefaultPeriodsSet()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var date = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var contract = new ContractWithMaximumTolerance();
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(date);
			var skillDay = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 10, 10);
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(23, 0, 24 + 7, 0));
			var otAvailability = new OvertimeAvailability(agent, date, TimeSpan.FromHours(24 + 7), TimeSpan.FromHours(24 + 8));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new IScheduleData[] { ass, otAvailability }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SkillActivity = activity,
				AvailableAgentsOnly = true
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(date) });

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().OvertimeActivities().Single().Period.StartDateTime.Date
				.Should().Be.EqualTo(date.Date.AddDays(1));
		}

		[Test]
		public void ShouldHandleOvertimeAvailableForNextDay()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity("_");
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var date = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var contract = new ContractWithMaximumTolerance();
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var shiftCategory = new ShiftCategory("_").WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(date);
			var skillDay = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 10, 10);
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(23, 0, 24 + 7, 0));
			var otAvailability = new OvertimeAvailability(agent, date.AddDays(1), TimeSpan.FromHours(7), TimeSpan.FromHours(8));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), new[] { agent }, new IScheduleData[] { ass, otAvailability }, skillDay);
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(23, 0, 40, 0),
				SelectedTimePeriod = new TimePeriod(0, 0, 1, 0),
				SkillActivity = activity,
				AvailableAgentsOnly = true
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(date) });

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().OvertimeActivities().Single().Period.StartDateTime.Date
				.Should().Be.EqualTo(date.Date.AddDays(1));
		}
		
		public void ShouldSchedulePersonOnDay()
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var activity = new Activity();
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			skill.TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var dateOnly = new DateOnly(2016, 7, 12);
			var scenario = new Scenario();
			var contract = new ContractWithMaximumTolerance();
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var shiftCategory = new ShiftCategory().WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromHours(10));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 0, 11, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), new[] { agent }, new[] { ass }, new[] { skillDay });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = definitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0)),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};
			
			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().Should().Not.Be.Empty();
		}

		[TestCase(true, ExpectedResult = false)]
		[TestCase(false, ExpectedResult = true)]
		public bool ShouldOnlyScheduleConnectedOvertimeTypes(bool haveSelectedDefinitionSet)
		{
			var definitionSet = new MultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime).WithId();
			var selectedDefinitionSet = new MultiplicatorDefinitionSet("selectedOvertime", MultiplicatorType.Overtime).WithId();
			var activity = new Activity();
			var skill = new Skill("_").DefaultResolution(60).For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			skill.TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var dateOnly = new DateOnly(2016, 7, 12);
			var scenario = new Scenario();
			var contract = new ContractWithMaximumTolerance();
			contract.AddMultiplicatorDefinitionSetCollection(definitionSet);
			if(haveSelectedDefinitionSet) contract.AddMultiplicatorDefinitionSetCollection(selectedDefinitionSet);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()).WithPersonPeriod(contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var shiftCategory = new ShiftCategory().WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromHours(10));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(10, 0, 11, 0));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1), new[] { agent }, new[] { ass }, new[] { skillDay });
			var overtimePreference = new OvertimePreferences
			{
				OvertimeType = selectedDefinitionSet,
				ScheduleTag = new ScheduleTag(),
				SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 6, 0, 0)),
				SelectedTimePeriod = new TimePeriod(1, 0, 1, 0),
				SkillActivity = activity
			};

			Target.Execute(overtimePreference, new NoSchedulingProgress(), new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) });

			return stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).OvertimeActivities().IsEmpty();
		}
	}
}