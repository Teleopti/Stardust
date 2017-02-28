﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class IntradayOptimizationTeamBlockDesktopTest : IConfigureToggleManager, ISetup
	{
		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		private readonly bool _resourcePlannerMaxSeatsNew40939;
		public OptimizationExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public IntradayOptimizationTeamBlockDesktopTest(bool resourcePlannerMaxSeatsNew40939)
		{
			_resourcePlannerMaxSeatsNew40939 = resourcePlannerMaxSeatsNew40939;
		}

		[Test]
		public void ShouldNotCrashWhenUsingKeepExistingDaysOff()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36)),
				PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9)
			};
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var schedulerStateHolderFrom = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferencesDefaultValueProvider().Fetch();
			optimizationPreferences.General.OptimizationStepShiftsWithinDay = true;
			optimizationPreferences.Extra.UseTeams = true;
			var daysOffPreferences = new DaysOffPreferences { UseKeepExistingDaysOff = true };

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(),
					schedulerStateHolderFrom,
					new List<IScheduleDay> { schedulerStateHolderFrom.Schedules[agent].ScheduledDay(dateOnly) },
					optimizationPreferences,
					new FixedDayOffOptimizationPreferenceProvider(daysOffPreferences));
			});
		}

		[Test]
		public void ShouldMarkDayToBeRecalculated()
		{
			if(!_resourcePlannerMaxSeatsNew40939)
				Assert.Ignore("Only interesting when MaxSeats toggle is on");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(16, 17));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = {UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats},
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false},
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Have.SameValuesAs(dateOnly);
		}

		[Test]
		public void ShouldNotMarkDayThatIsNotChangedToBeRecalculated()
		{
			if (!_resourcePlannerMaxSeatsNew40939)
				Assert.Ignore("Only interesting when MaxSeats toggle is on");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(16, 17));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldMarkDayToBeRecalculatedWhenDoNotBreak()
		{
			if (!_resourcePlannerMaxSeatsNew40939)
				Assert.Ignore("Only interesting when MaxSeats toggle is on");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), shiftCategory));
			var agentScheduledOneHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(team);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var assOneHour = new PersonAssignment(agentScheduledOneHour, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(16, 17));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent, agentScheduledOneHour },
															new[] { ass, assOneHour },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Have.SameValuesAs(dateOnly);
		}

		[Test]
		public void ShouldNotMarkDayToBeRecalculatedWhenDoNotBreak()
		{
			if (!_resourcePlannerMaxSeatsNew40939)
				Assert.Ignore("Only interesting when MaxSeats toggle is on");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var site = new Site("siten") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithId().WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team).WithSchedulePeriodOneDay(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(9, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(),
															new[] { agent },
															new[] { ass },
															Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak },
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = false },
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, optPreferences, null);

			stateHolder.DaysToRecalculate
				.Should().Be.Empty();
		}

		[RemoveMeWithToggle("Should not be necessary when toggle is on/removed", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerMaxSeatsNew40939)
				toggleManager.Enable(Toggles.ResourcePlanner_MaxSeatsNew_40939);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
		}
	}
}