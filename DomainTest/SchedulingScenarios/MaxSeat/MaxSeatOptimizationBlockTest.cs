﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat.TestData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class MaxSeatOptimizationBlockTest
	{
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;

		[Test]
		public void ShouldNotMoveMoreSchedulesThanNecessary()
		{
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agent2Data = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Assignment, agentData.Assignment, agent2Data.Assignment });
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agent2Data.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotCrashWhenBlockSameShiftCategoryIsUsedWhenOptimizationWasUnsuccesful()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategoryInRuleset = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategoryInRuleset));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet);
			agent.Period(DateOnly.MinValue).Team = team;
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 2));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(8, 16));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)), new[] { agentScheduledForAnHourData.Assignment, ass });
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Extra.UseBlockSameShiftCategory = true;

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {agent}, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
			});
		}

		[Test]
		public void ShouldNotCrashWhenBlockSameShiftIsUsedWhenOptimizationWasUnsuccesful()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategoryInRuleset = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategoryInRuleset));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet);
			agent.Period(DateOnly.MinValue).Team = team;
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 2));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(8, 16));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)), new[] { agentScheduledForAnHourData.Assignment, ass });
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Extra.UseBlockSameShift = true;

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
			});
		}

		[Test]
		public void ShouldNotCrashWhenBlockContainsUnlockedDaysWithNoMatrixes()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategoryInRuleset = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategoryInRuleset));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet);
			agent.Period(DateOnly.MinValue).Team = team;
			agent.AddSchedulePeriod(new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 2));
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(8, 16));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)), new[] { agentScheduledForAnHourData.Assignment, ass });
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Extra.UseBlockSameShift = true;
			optPreferences.Extra.BlockTypeValue = BlockFinderType.BetweenDayOff;

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
			});
		}
	}
}