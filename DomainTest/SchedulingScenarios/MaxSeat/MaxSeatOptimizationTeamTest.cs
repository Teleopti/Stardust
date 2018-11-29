using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat.TestData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	public class MaxSeatOptimizationTeamTest : MaxSeatScenario
	{
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;
		public FakeRuleSetBagRepository RuleSetBagRepository;


		[Test]
		public void ShouldChooseShiftForAllAgentsInTeam()
		{
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentScheduledForAnHourData.Assignment });

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent }, schedules, Enumerable.Empty<ISkillDay>(), DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team), null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotBeForcedToUseSameShiftIfDifferentTeams()
		{
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site}, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentScheduledForAnHourData.Assignment });

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent }, schedules, Enumerable.Empty<ISkillDay>(), DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team), null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCheckAllMembersMaxSeatPeak()
		{
			var siteWithSeats = new Site("_") { MaxSeats = 10 }.WithId();
			var siteWithNoSeats = new Site("_") {MaxSeats = 1}.WithId();
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()))) { Description = new Description("_")};
			RuleSetBagRepository.Add(ruleSetBag);
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team {Site = siteWithNoSeats}, ruleSetBag, scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentWithAvailableSeats = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = siteWithSeats }, ruleSetBag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentThatNeedsToBeMoved = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = siteWithNoSeats }, ruleSetBag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentWithAvailableSeats.Assignment, agentThatNeedsToBeMoved.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team);
			optPreferences.Extra.TeamGroupPage = new GroupPageLight("_", GroupPageType.RuleSetBag);

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentWithAvailableSeats.Agent, agentThatNeedsToBeMoved.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentThatNeedsToBeMoved.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldNotChangeDayWhereMaxSeatIsOkWhenMultipleDaysSelected()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(period.EndDate, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDatas = MaxSeatDataFactory.CreateAgentWithAssignments(period, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));

			var assignments = agentDatas.Select(maxSeatData => maxSeatData.Assignment).ToList();
			assignments.Add(agentDataOneHour.Assignment);

			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, assignments);
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team);

			Target.Optimize(new NoSchedulingProgress(), period, new[] { agentDatas.First().Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentDatas.First().Agent].ScheduledDay(period.StartDate)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldNotRemoveSchedulesForNonOptimizedAgentsWhenDoNotBreakIsUsed()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHour3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));

			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new [] { agentData1.Assignment, agentDataOneHour1.Assignment, agentDataOneHour2.Assignment, agentDataOneHour3.Assignment });
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team);
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentDataOneHour1.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(16));
			schedules[agentDataOneHour2.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(16));
			schedules[agentDataOneHour3.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(16));
		}

		[Test]
		public void ShouldNotCrashWhenEmptyDayInMultipleDaysSelected()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(period.EndDate, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(period.EndDate, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDatas = MaxSeatDataFactory.CreateAgentWithAssignments(period, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var assignments = agentDatas.Select(maxSeatData => maxSeatData.Assignment).ToList();
			assignments[1].Clear();
			assignments.Add(agentDataOneHour1.Assignment);
			assignments.Add(agentDataOneHour2.Assignment);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, assignments);
			var optPreferences = DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team);

			Target.Optimize(new NoSchedulingProgress(),  period, new[] { agentDatas.First().Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentDatas.First().Agent].ScheduledDay(period.StartDate)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldNotCrashWhenEmptyDayOnOneAgentAndDayOffOnOtherAgent()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(period.StartDate, team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(period.StartDate, team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(period.EndDate, team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(period.EndDate, team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
		
			agentData1.Assignment.Clear();
			agentData2.Assignment.SetDayOff(new DayOffTemplate());
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, new [] {agentDataOneHour1.Assignment, agentDataOneHour2.Assignment, agentData1.Assignment, agentData2.Assignment});
			
			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(new NoSchedulingProgress(),  dateOnly.AddDays(1).ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent }, schedules, Enumerable.Empty<ISkillDay>(), DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team), null);
			});
		}
		
		[Test]
		public void ShouldHandleMixOfTerminatedAndNonTerminatedAgentsInSameTeam()
		{
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity { RequiresSeat = true }.WithId();
			var date = new DateOnly(2016, 10, 25);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(date, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentTerminatedData = MaxSeatDataFactory.CreateAgentWithAssignment(date, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentNonTerminatedDatas = MaxSeatDataFactory.CreateAgentWithAssignments(new DateOnlyPeriod(date, date.AddDays(1)), team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var asses = agentNonTerminatedDatas.Select(x => x.Assignment).Union(new[] {agentTerminatedData.Assignment, agentScheduledForAnHourData.Assignment});
			agentTerminatedData.Agent.TerminatePerson(date, new PersonAccountUpdaterDummy(), new DummyPersonLeavingUpdater());
			var nonTerminatedAgent = agentNonTerminatedDatas.First().Agent;
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, asses);

			Target.Optimize(new NoSchedulingProgress(), period, new[] { agentTerminatedData.Agent, nonTerminatedAgent }, schedules, Enumerable.Empty<ISkillDay>(), DefaultMaxSeatOptimizationPreferences.Create(TeamBlockType.Team), null);

			schedules[nonTerminatedAgent].ScheduledDay(date).PersonAssignment().Period.StartDateTime.TimeOfDay.Hours
				.Should().Be.EqualTo(9);
		}
	}
}