using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat.TestData;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	[TestFixture(TeamBlockType.Team)]
	[TestFixture(TeamBlockType.Block)]
	public class MaxSeatRestrictionsTest : MaxSeatScenario
	{
		private readonly TeamBlockType _teamBlockType;
		public MaxSeatOptimization Target;
		public FakeTeamRepository TeamRepository;

		public MaxSeatRestrictionsTest(TeamBlockType teamBlockType)
		{
			_teamBlockType = teamBlockType;
		}

		[Test]
		public void ShouldConsiderShiftCategoryLimitations()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var shiftCategoryLimitation = new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 0 };
			agentData.Agent.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(shiftCategoryLimitation);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.UseShiftCategoryLimitations = true;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().ShiftCategory.Equals(shiftCategory))
				.Should().Be.EqualTo(0);
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderPreferences(double preferenceValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var preferenceRestriction = new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)) };
			var preferenceDay = new PreferenceDay(agentData.Agent, dateOnly, preferenceRestriction);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new IPersistableScheduleData[] { agentData.Assignment, agentScheduledForAnHourData.Assignment, preferenceDay });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.UsePreferences = true;
			optPreferences.General.PreferencesValue = preferenceValue;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderAvailabilities(double availabilityValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var availabilityResctriction = new AvailabilityRestriction { StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)) };
			var personRestriction = new ScheduleDataRestriction(agentData.Agent, availabilityResctriction, dateOnly);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new IScheduleData[] { agentData.Assignment, agentScheduledForAnHourData.Assignment, personRestriction });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.UseAvailabilities = true;
			optPreferences.General.AvailabilitiesValue = availabilityValue;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderRotations(double rotationValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var rotationRestriction = new RotationRestriction() { StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)) };
			var personRestriction = new ScheduleDataRestriction(agentData.Agent, rotationRestriction, dateOnly);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new IScheduleData[] { agentData.Assignment, agentScheduledForAnHourData.Assignment, personRestriction });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.UseRotations = true;
			optPreferences.General.RotationsValue = rotationValue;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderHourlyAvailabilities(double availabilityValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction { StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)) };
			var studentAvailabilityDay = new StudentAvailabilityDay(agentData.Agent, dateOnly, new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction });
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new IPersistableScheduleData[] { agentData.Assignment, agentScheduledForAnHourData.Assignment, studentAvailabilityDay });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.UseStudentAvailabilities = true;
			optPreferences.General.StudentAvailabilitiesValue = availabilityValue;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderMustHaves(double mustHaveValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Site = site }.WithDescription(new Description("_"));
			TeamRepository.HasConnectedToCurrentBusinessUnit(team);
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var preferenceRestriction = new PreferenceRestriction { MustHave = true, StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0)) };
			var preferenceDay = new PreferenceDay(agentData.Agent, dateOnly, preferenceRestriction);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new IPersistableScheduleData[] { agentData.Assignment, agentScheduledForAnHourData.Assignment, preferenceDay });
			var optPreferences = createOptimizationPreferences();
			optPreferences.General.UseMustHaves = true;
			optPreferences.General.MustHavesValue = mustHaveValue;

			Target.Optimize(new NoSchedulingProgress(),  dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences, null);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		private OptimizationPreferences createOptimizationPreferences()
		{
			return DefaultMaxSeatOptimizationPreferences.Create(_teamBlockType);
		}
	}
}