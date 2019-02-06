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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat.TestData;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	[TestFixture(TeamBlockType.Team)]
	[TestFixture(TeamBlockType.Block)]
	public class MaxSeatSurroundingDaysTest : MaxSeatScenario
	{
		private readonly TeamBlockType _teamBlockType;
		public MaxSeatOptimization Target;
		public FakeTeamRepository TeamRepository;

		public MaxSeatSurroundingDaysTest(TeamBlockType teamBlockType)
		{
			_teamBlockType = teamBlockType;
		}

		[Test]
		public void ShouldFixMaxSeatIfIssueIsNotOnFirstDay()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
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
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(),  period, new[] { agentDatas.First().Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentDatas.First().Agent].ScheduledDay(period.EndDate)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldCheckMaxPeaksOnNightShifts()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(18, 0, 18, 0, 60), new TimePeriodWithSegment(26, 0, 26, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(10, 0, 11, 0));
			var agentDataOneHourAtNight = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly.AddDays(1), team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(0, 0, 1, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new[] { agentData.Assignment, agentDataOneHour.Assignment, agentDataOneHourAtNight.Assignment });

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), createOptimizationPreferences(), null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9)); //should not have been touched
		}

		[TestCase(1)]
		[TestCase(2)]
		public void ShouldCalculateMaxPeakOnNextDay(int ruleSetOrder)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team {  Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(18, 0, 18, 0, 60), new TimePeriodWithSegment(26, 0, 26, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(02, 0, 02, 0, 60), new TimePeriodWithSegment(10, 0, 10, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag();

			if (ruleSetOrder.Equals(1))
			{
				ruleSetBag.AddRuleSet(ruleSet1);
				ruleSetBag.AddRuleSet(ruleSet2);
			}

			if (ruleSetOrder.Equals(2))
			{
				ruleSetBag.AddRuleSet(ruleSet2);
				ruleSetBag.AddRuleSet(ruleSet1);
			}

			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, ruleSetBag, scenario, activity, new TimePeriod(10, 0, 11, 0));
			var agentDataOneHourAtNight = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly.AddDays(1), team, ruleSetBag, scenario, activity, new TimePeriod(0, 0, 1, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, ruleSetBag, scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new[] { agentData.Assignment, agentDataOneHour.Assignment, agentDataOneHourAtNight.Assignment });

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), createOptimizationPreferences(), null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(2));
		}

		[Test]
		public void ShouldFixMaxPeaksComingFromNightShifts()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team {  Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var agentDataOneHourAtNight = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly.AddDays(1), team, new RuleSetBag(), scenario, activity, new TimePeriod(0, 0, 1, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(), scenario, activity, new TimePeriod(18, 0, 26, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new[] { agentData.Assignment, agentDataOneHourAtNight.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftLayers
				.Should().Be.Empty();
		}


		[Test]
		public void ShouldHandleAgentsInUtcTimeZoneYesterday()
		{
			var agentTimeZone = TimeZoneInfoFactory.SingaporeTimeZoneInfo();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team {  Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(1, 0, 1, 0, 60), new TimePeriodWithSegment(9, 0, 9, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(0, 0, 1, 0), agentTimeZone);
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(0, 0, 8, 0), agentTimeZone);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), createOptimizationPreferences(), null);

			var agentStartTimeLocal = TimeZoneHelper.ConvertFromUtc(schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime, agentTimeZone);
			agentStartTimeLocal.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldNotConsiderPeaksOnIntersectingDays()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDayBefore1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly.AddDays(-1), team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDayBefore2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly.AddDays(-1), team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly.AddDays(1)), new[] { agentData.Assignment, agentDataOneHour.Assignment, agentDayBefore1.Assignment, agentDayBefore2.Assignment });

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), createOptimizationPreferences(), null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[TestCase(1)]
		[TestCase(2)]
		public void ShouldCalculateMaxPeakOnDayBefore(int ruleSetOrder)
		{
			var agentTimeZone = TimeZoneInfoFactory.SingaporeTimeZoneInfo();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team {  Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(1, 0, 1, 0, 60), new TimePeriodWithSegment(9, 0, 9, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(09, 0, 09, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag();

			if (ruleSetOrder.Equals(1))
			{
				ruleSetBag.AddRuleSet(ruleSet1);
				ruleSetBag.AddRuleSet(ruleSet2);
			}

			if (ruleSetOrder.Equals(2))
			{
				ruleSetBag.AddRuleSet(ruleSet2);
				ruleSetBag.AddRuleSet(ruleSet1);
			}

			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, ruleSetBag, scenario, activity, new TimePeriod(1, 0, 2, 0), agentTimeZone);
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, ruleSetBag, scenario, activity, new TimePeriod(0, 0, 8, 0), agentTimeZone);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new[] { agentData.Assignment, agentDataOneHour.Assignment });

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), createOptimizationPreferences(), null);

			var agentStartTimeLocal = TimeZoneHelper.ConvertFromUtc(schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime, agentTimeZone);
			agentStartTimeLocal.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}

		private OptimizationPreferences createOptimizationPreferences()
		{
			return DefaultMaxSeatOptimizationPreferences.Create(_teamBlockType);
		}
	}
}