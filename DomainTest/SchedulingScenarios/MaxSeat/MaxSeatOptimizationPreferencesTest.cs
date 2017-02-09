using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
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
	public class MaxSeatOptimizationPreferencesTest
	{
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;

		[Test]
		public void ShouldRespectKeepShiftCategoriesTeamHierarchyOneAgent()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), newShiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new [] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = new OptimizationPreferences
			{
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) },
				Shifts = { KeepShiftCategories = true },
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats }
			};

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftCategory
				.Should().Not.Be.EqualTo(newShiftCategory);
		}

		[Test]
		public void ShouldRespectKeepShiftCategoriesSingleAgent()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), newShiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = new OptimizationPreferences
			{
				Extra = { UseTeams = false, UseTeamBlockOption = false, TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent) },
				Shifts = { KeepShiftCategories = true },
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats }
			};

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftCategory.Should().Not.Be.EqualTo(newShiftCategory);
		}

		[Test]
		public void ShouldRespectKeepShiftCategoriesTeamHierarchyTwoAgents()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), newShiftCategory));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(0, 0, 0, 0, 60), new TimePeriodWithSegment(8, 0, 8, 0, 60), newShiftCategory));
			var agentScheduledForAnHourData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet2), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentScheduledForAnHourData1.Assignment });
			var optPreferences = new OptimizationPreferences
			{
				Extra = { UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy) },
				Shifts = { KeepShiftCategories = true },
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats }
			};

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData1.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftCategory.Should().Not.Be.EqualTo(newShiftCategory);
			schedules[agentData2.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftCategory.Should().Not.Be.EqualTo(newShiftCategory);
		}

		[Test]
		public void ShouldRespectKeepShiftCategoriesBlockTwoDays()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), newShiftCategory));
			var agentScheduledForAnHourDatas = MaxSeatDataFactory.CreateAgentWithAssignments(period, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDatas = MaxSeatDataFactory.CreateAgentWithAssignments(period, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0)).ToArray();
			var agent = agentDatas.First().Agent;
			var agentScheduledForAnHourAsses = agentScheduledForAnHourDatas.Select(maxSeatData => maxSeatData.Assignment).Cast<IScheduleData>();
			var agentAsses = agentDatas.Select(maxSeatData => maxSeatData.Assignment).Cast<IScheduleData>();
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, agentAsses.Union(agentScheduledForAnHourAsses));
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Shifts.KeepShiftCategories = true;
			
			Target.Optimize(new NoSchedulingProgress(), period, new[] { agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agent].ScheduledDay(dateOnly).PersonAssignment().ShiftCategory.Should().Not.Be.EqualTo(newShiftCategory);
			schedules[agent].ScheduledDay(dateOnly.AddDays(1)).PersonAssignment().ShiftCategory.Should().Not.Be.EqualTo(newShiftCategory);
		}

		[Test]
		public void ShouldRespectKeepShiftSingleAgent()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), newShiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });

			var optPreferences = new OptimizationPreferences
			{
				Extra = { UseTeams = false, UseTeamBlockOption = false, TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent) },
				Shifts = { KeepShifts = true, KeepShiftsValue = 100},
				Advanced = { UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats }
			};

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay.Should().Be.EqualTo(new TimeSpan(8, 0, 0));
		}
	}
}