using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat.TestData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	[TestFixture(TeamBlockType.Team)]
	[TestFixture(TeamBlockType.Block)]
	public class MaxSeatOptimizationTest : MaxSeatScenario
	{
		private readonly TeamBlockType _teamBlockType;
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;
		public Func<IGridlockManager> LockManager;

		public MaxSeatOptimizationTest(TeamBlockType teamBlockType)
		{
			_teamBlockType = teamBlockType;
		}

		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeats)]
		public void HappyPath(MaxSeatsFeatureOptions maxSeatsFeatureOptions)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = maxSeatsFeatureOptions;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void HappyPathShouldWorkWhenAgentExistsWithoutPersonPeriod()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var agentWithNoPersonPeriod = new Person();
			var someUninterestingAssignmentToIncludeAgentWithNoPersonPeriod = new PersonAssignment(agentWithNoPersonPeriod, scenario, dateOnly);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment, someUninterestingAssignmentToIncludeAgentWithNoPersonPeriod });

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), createOptimizationPreferences(), null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldNotRollbackEveryChangeIfLastOneBreakRule()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentThatWillBeOptimized = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentWillBreakShiftCategory = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var shiftCategoryLimitation = new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 0 };
			agentWillBreakShiftCategory.Agent.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(shiftCategoryLimitation);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentThatWillBeOptimized.Assignment, agentWillBreakShiftCategory.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.UseShiftCategoryLimitations = true;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentThatWillBeOptimized.Agent, agentWillBreakShiftCategory.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSetCorrectTag()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.ScheduleTag = new ScheduleTag();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).ScheduleTag()
				.Should().Be.SameInstanceAs(optPreferences.General.ScheduleTag);
		}

		[Test]
		public void ShouldDoNothingWhenNotOverMaxSeatLimit()
		{
			var site = new Site("_") { MaxSeats = 10 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldDoNothingWhenMaxSeatDontGetBetter()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour1.Assignment, agentDataOneHour2.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldDoNothingWhenMaxSeatGetsEqual()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour1.Assignment, agentDataOneHour2.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldCheckEachIntervalOnShift()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour1.Assignment, agentDataOneHour2.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldChooseShiftWherePeakIsLowest()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHourEarly1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOneHourEarly2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOneHourEarly3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOneHourLate1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHourLate2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), 
				new[]
				{
					agentData.Assignment, agentDataOneHourEarly1.Assignment, agentDataOneHourEarly2.Assignment, agentDataOneHourEarly3.Assignment, agentDataOneHourLate1.Assignment, agentDataOneHourLate2.Assignment
				});
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldChooseShiftWithLowestPeakWhenMultipleShifts()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 18, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8),OperatorLimiter.Equals));
			var agentData8To9A = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData8To9B = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData8To9C = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData9To10A = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 10, 0));
			var agentData9To10B = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 10, 0));
			var agentData9To10C = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 10, 0));
			var agentData10To11A = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(10, 0, 11, 0));
			var agentData10To11B = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(10, 0, 11, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(),
				new[]
				{
					agentData.Assignment, agentData8To9A.Assignment, agentData8To9B.Assignment, agentData8To9C.Assignment,agentData9To10A.Assignment, agentData9To10B.Assignment, agentData9To10C.Assignment, agentData10To11A.Assignment, agentData10To11B.Assignment
				});
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(10));
		}

		[Test]
		public void ShouldDoNothingWhenNoMaxSeatLimit()
		{
			var site = new Site("_").WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotCrashWhenMixingAgentOnMaxSeatSiteWithAgentOnSiteWithNoMaxSeat()
		{
			var siteMaxSeat = new Site("MaxSeat") { MaxSeats = 1 }.WithId();
			var site = new Site("_").WithId();
			var teamMaxSeat = new Team { Site = siteMaxSeat }.WithDescription(new Description("_"));
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			var bu = BusinessUnitFactory.CreateBusinessUnitAndAppend(teamMaxSeat, team);
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(bu);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataMaxSeat = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamMaxSeat, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentDataMaxSeat.Assignment, agentData.Assignment});
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;

			Assert.DoesNotThrow(() =>
			{
				Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentDataMaxSeat.Agent, agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
			});
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldConsiderActivityRequireSeat(bool ruleSetOrder)
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var activityRequireNoSeat = new Activity("_") { RequiresSeat = false }.WithId();
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSetNotRequireSeat = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityRequireNoSeat, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag();
			if (ruleSetOrder)
			{
				ruleSetBag.AddRuleSet(ruleSet);
				ruleSetBag.AddRuleSet(ruleSetNotRequireSeat);
			}
			else
			{
				ruleSetBag.AddRuleSet(ruleSetNotRequireSeat);
				ruleSetBag.AddRuleSet(ruleSet);
			}
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, ruleSetBag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment });

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), createOptimizationPreferences(), null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftLayers.First().Payload.RequiresSeat.Should().Be.False();
		}

		[Test]
		public void ShouldDoNothingWhenNotUsingMaxSeatLimit()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Assignment, agentData.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.DoNotConsiderMaxSeats;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotOptimizeAgentNotHavingTheMaxSeatSkillThatIsOverLimit()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var siteOverLimit = new Site("_") { MaxSeats = 1 }.WithId();
			var teamOverLimit = new Team { Site = siteOverLimit }.WithDescription(new Description("_"));
			var siteUnderLimit = new Site("_") { MaxSeats = 10 }.WithId();
			var teamUnderLimit = new Team { Site = siteUnderLimit }.WithDescription(new Description("_"));
			var loggedOnBu = new BusinessUnit("_");
			loggedOnBu.AddSite(siteOverLimit);
			loggedOnBu.AddSite(siteUnderLimit);
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(loggedOnBu);
			var dateOnly = new DateOnly(2016, 11, 1);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamOverLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentScheduledForAnHourData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamOverLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataSiteUnderLimit = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamUnderLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentDataSiteUnderLimit.Assignment, agentScheduledForAnHourData2.Assignment,agentScheduledForAnHourData1.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentDataSiteUnderLimit.Agent, agentScheduledForAnHourData1.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentDataSiteUnderLimit.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldNotCareAboutNotInvolvedMaxSeatSkill()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var siteOverLimit = new Site("_") { MaxSeats = 1 }.WithId();
			var teamOverLimit = new Team { Site = siteOverLimit }.WithDescription(new Description("_"));
			var siteUnderLimit = new Site("_") { MaxSeats = 10 }.WithId();
			var teamUnderLimit = new Team { Site = siteUnderLimit }.WithDescription(new Description("_"));
			var loggedOnBu = new BusinessUnit("_");
			loggedOnBu.AddSite(siteOverLimit);
			loggedOnBu.AddSite(siteUnderLimit);
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(loggedOnBu);
			var dateOnly = new DateOnly(2016, 11, 1);
			var scenario = new Scenario("_");
			//8-9 no seats available, 16-17 seats available
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentScheduledForAnHourData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamUnderLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentScheduledForAnHourData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamUnderLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentScheduledForAnHourData3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamUnderLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentScheduledForAnHourData4 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamOverLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOverLimit = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, teamOverLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentDataOverLimit.Assignment, agentScheduledForAnHourData1.Assignment, agentScheduledForAnHourData2.Assignment, agentScheduledForAnHourData3.Assignment, agentScheduledForAnHourData4.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentDataOverLimit.Agent, agentScheduledForAnHourData1.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentDataOverLimit.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}


		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, ExpectedResult = false)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeats, ExpectedResult = true)]
		public bool ShouldNotBreakMaxSeat(MaxSeatsFeatureOptions maxSeatsFeatureOptions)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = maxSeatsFeatureOptions;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			return schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Any();
		}

		[Test]
		public void ShouldOnlyRemoveScheduleAffectingOverMaxSeatLimit()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 22, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentData3.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent, agentData3.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData3.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldTryToFixMaxSeatForAllBeforeRemovingShifts()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team1 = new Team { Site = site }.WithDescription(new Description("_"));
			var team2 = new Team { Site = site }.WithDescription(new Description("_"));

			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team1));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(16, 0, 16, 0, 60), new TimePeriodWithSegment(24, 0, 24, 0, 60), new ShiftCategory("_").WithId()));

			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team2, new RuleSetBag(ruleSet2), scenario, activity, new TimePeriod(8, 0, 16, 0));

			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent}, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData1.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotCrashWhenOptimizeEmptyDays()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team1 = new Team { Site = site }.WithDescription(new Description("_"));

			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team1));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));
			agentData1.Assignment.Clear();
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));

			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] {agentData1.Assignment, agentData2.Assignment, agentData3.Assignment});
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent}, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
		}

		[Test]
		public void ShouldNotCrashWhenShiftLayerDoesNotMatchSkillStaffPeriod()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 10, 8, 10, 60), new TimePeriodWithSegment(16, 10, 16, 10, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PeakShouldNotBe0ForNonSkillIntervals(bool ruleSet1First)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSetBag = new RuleSetBag();
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 5, 8, 5, 60), new TimePeriodWithSegment(16, 5, 16, 5, 60), new ShiftCategory("_").WithId()));
			if (ruleSet1First)
			{
				ruleSetBag.AddRuleSet(ruleSet1);
				ruleSetBag.AddRuleSet(ruleSet2);
			}
			else
			{
				ruleSetBag.AddRuleSet(ruleSet2);
				ruleSetBag.AddRuleSet(ruleSet1);
			}
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, ruleSetBag, scenario, activity, new TimePeriod(10, 0, 18, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldNotOptimizeEmptyDays()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team1 = new Team { Site = site }.WithDescription(new Description("_"));

			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team1));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(17, 0, 17, 0, 60), new TimePeriodWithSegment(25, 0, 25, 0, 60), new ShiftCategory("_").WithId()));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));
			agentData1.Assignment.Clear();
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet2), scenario, activity, new TimePeriod(16, 0, 24, 0));
			var agentData3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet2), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentData3.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent}, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData1.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotRemoveMoreThanNecessary()
		{
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new NoSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules.SchedulesForDay(dateOnly).Count(x => x.IsScheduled()).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotThouchLockedDays()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
			LockManager().AddLock(agentData.Agent, dateOnly, LockType.Normal);

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}

		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeats)]
		public void ShouldReportProgress(MaxSeatsFeatureOptions maxSeatsFeatureOptions)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = maxSeatsFeatureOptions;
			var progress = new TrackSchedulingProgress<ResourceOptimizerProgressEventArgs>();
			
			Target.Optimize(progress, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			progress.ReportedProgress.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReportRemoveProgress()
		{
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
			var progress = new TrackSchedulingProgress<ResourceOptimizerProgressEventArgs>();

			Target.Optimize(progress, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			progress.ReportedProgress.Select(x=> x.Message).Should().Contain(Resources.DoNotBreakMaxSeatDotDotDot);
		}

		[Test]
		public void ShouldCancel()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
			
			Target.Optimize(new FakeCloseSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));	
		}

		[Test]
		public void ShouldCancelAtRemove()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(8, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(new FakeCloseSchedulingProgress(), dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}

		private OptimizationPreferences createOptimizationPreferences()
		{
			return DefaultMaxSeatOptimizationPreferences.Create(_teamBlockType);
		}
	}
}