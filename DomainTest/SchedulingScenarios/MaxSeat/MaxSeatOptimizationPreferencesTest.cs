using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat.TestData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	public class MaxSeatOptimizationPreferencesTest : MaxSeatScenario, IIsolateSystem
	{
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;
		public FakeUserTimeZone UserTimeZone;

		[Test]
		public void ShouldAllowModifyingBlockWithMultipleDays()
		{
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var oldShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_") { RequiresSeat = false }.WithId(), new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), newShiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodTwoDays(dateOnly).WithPersonPeriod(ruleSet, team);
			var asses = period.DayCollection().Select(date => new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(oldShiftCategory));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, asses);
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
			optPreferences.Shifts.ActivityToKeepLengthOn = new Activity("_");
			optPreferences.Shifts.KeepActivityLength = true;

			Target.Optimize(new NoSchedulingProgress(), period, new[] {agent}, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agent].ScheduledDayCollection(period).All(x => x.PersonAssignment().ShiftCategory.Equals(newShiftCategory))
				.Should().Be.True();
		}

		[Test]
		public void ShouldRespectAlterBetweenBlockTwoDays()
		{
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			var activityRequiresSeat = new Activity("_") { RequiresSeat = true }.WithId();
			var activityRequiresNoSeat = new Activity("_") { RequiresSeat = false }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var oldShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityRequiresNoSeat, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), newShiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodTwoDays(dateOnly).WithPersonPeriod(ruleSet, team);
			var asses = period.DayCollection().Select(date => new PersonAssignment(agent, scenario, date).WithLayer(activityRequiresSeat, new TimePeriod(8, 16)).ShiftCategory(oldShiftCategory));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, asses);
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
			optPreferences.Shifts.SelectedTimePeriod = new TimePeriod(8, 17);
			optPreferences.Shifts.AlterBetween = true;

			Target.Optimize(new NoSchedulingProgress(), period, new[] { agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agent].ScheduledDayCollection(period).All(x => x.PersonAssignment().ShiftCategory.Equals(newShiftCategory))
				.Should().Be.True();
		}

		[TestCase("UTC")]
		[TestCase("W. Europe Standard Time")]
		[TestCase("Mountain Standard Time")]
		[TestCase("Singapore Standard Time")]
		public void ShouldRespectAlterBetweenBlockTwoDaysEndAt24(string timeZone)
		{
			UserTimeZone.Is(TimeZoneInfo.FindSystemTimeZoneById(timeZone));
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			var activityRequiresSeat = new Activity("_") { RequiresSeat = true }.WithId();
			var activityRequiresNoSeat = new Activity("_") { RequiresSeat = false }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var oldShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityRequiresNoSeat, new TimePeriodWithSegment(8, 0, 9, 0, 15), new TimePeriodWithSegment(16, 0, 17, 0, 15), newShiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodTwoDays(dateOnly).WithPersonPeriod(ruleSet, team);
			var asses = period.DayCollection().Select(date => new PersonAssignment(agent, scenario, date).WithLayer(activityRequiresSeat, new TimePeriod(7, 15)).ShiftCategory(oldShiftCategory));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, asses);
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
			optPreferences.Shifts.SelectedTimePeriod = new TimePeriod(0, 24);
			optPreferences.Shifts.AlterBetween = true;

			Target.Optimize(new NoSchedulingProgress(), period, new[] { agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agent].ScheduledDayCollection(period).All(x => x.PersonAssignment().ShiftCategory.Equals(newShiftCategory))
				.Should().Be.True();
		}

		[Test]
		public void ShouldHandleAlterBetweenWhenPersonAssignmentMissingInSchedulePeriod()
		{
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			var activityRequiresSeat = new Activity("_") { RequiresSeat = true }.WithId();
			var activityRequiresNoSeat = new Activity("_") { RequiresSeat = false }.WithId();
			var date = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var newShiftCategory = new ShiftCategory("_").WithId();
			var oldShiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityRequiresNoSeat, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), newShiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodTwoDays(date).WithPersonPeriod(ruleSet, team);
			var ass =  new PersonAssignment(agent, scenario, date).WithLayer(activityRequiresSeat, new TimePeriod(8, 16)).ShiftCategory(oldShiftCategory);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, date.ToDateOnlyPeriod(), new [] {ass});
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
			optPreferences.Shifts.SelectedTimePeriod = new TimePeriod(0, 24);
			optPreferences.Shifts.AlterBetween = true;

			Target.Optimize(new NoSchedulingProgress(), date.ToDateOnlyPeriod(), new[] { agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agent].ScheduledDay(date).PersonAssignment().ShiftCategory.Equals(newShiftCategory)
					.Should().Be.True();
		}

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
				Shifts = { KeepStartTimes = true  },
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
				Shifts = { KeepEndTimes = true },
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
			var agentScheduledForAnHourAsses = agentScheduledForAnHourDatas.Select(maxSeatData => maxSeatData.Assignment);
			var agentAsses = agentDatas.Select(maxSeatData => maxSeatData.Assignment);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, agentAsses.Union(agentScheduledForAnHourAsses));
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Block);
			optPreferences.Shifts.KeepShiftCategories = true;
			
			Target.Optimize(new NoSchedulingProgress(), period, new[] { agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agent].ScheduledDay(dateOnly).PersonAssignment().ShiftCategory.Should().Not.Be.EqualTo(newShiftCategory);
			schedules[agent].ScheduledDay(dateOnly.AddDays(1)).PersonAssignment().ShiftCategory.Should().Not.Be.EqualTo(newShiftCategory);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
		}
	}
}