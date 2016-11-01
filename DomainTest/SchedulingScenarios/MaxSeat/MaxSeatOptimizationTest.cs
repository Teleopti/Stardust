using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public abstract class MaxSeatOptimizationTest
	{
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;

		[Test]
		public void ShouldConsiderShiftCategoryLimitations()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), shiftCategory));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var shiftCategoryLimitation = new ShiftCategoryLimitation(shiftCategory) {MaxNumberOf = 0};
			agentData.Agent.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(shiftCategoryLimitation);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.UseShiftCategoryLimitations = true;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().ShiftCategory.Equals(shiftCategory))
				.Should().Be.EqualTo(0);
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderPreferences(double preferenceValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
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
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.UsePreferences = true;
			optPreferences.General.PreferencesValue = preferenceValue;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderAvailabilities(double availabilityValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
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
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.UseAvailabilities = true;
			optPreferences.General.AvailabilitiesValue = availabilityValue;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderRotations(double rotationValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
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
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.UseRotations = true;
			optPreferences.General.RotationsValue = rotationValue;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderHourlyAvailabilities(double availabilityValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
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
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.UseStudentAvailabilities = true;
			optPreferences.General.StudentAvailabilitiesValue = availabilityValue;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[TestCase(1d, ExpectedResult = 0)]
		[TestCase(0d, ExpectedResult = 1)]
		public int ShouldConsiderMustHaves(double mustHaveValue)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
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
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.UseMustHaves = true;
			optPreferences.General.MustHavesValue = mustHaveValue;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			return schedules.SchedulesForDay(dateOnly).Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldDoNothingWhenNotOverMaxSeatLimit()
		{
			var site = new Site("_") { MaxSeats = 10 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment });
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldDoNothingWhenMaxSeatDontGetBetter()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour1.Assignment, agentDataOneHour2.Assignment });
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldCheckEachIntervalOnShift()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour1.Assignment, agentDataOneHour2.Assignment });
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldChooseShiftWherePeakIsLowest()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
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
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldChooseShiftWithLowestPeakWhenMultipleShifts()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 10, 0, 60), new TimePeriodWithSegment(17, 0, 18, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8),OperatorLimiter.Equals));
			var agentData8To91 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData8To92 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData8To93 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData9To101 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 10, 0));
			var agentData9To102 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 10, 0));
			var agentData9To103 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 10, 0));
			var agentData10To111 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(10, 0, 11, 0));
			var agentData10To112 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(10, 0, 11, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(),
				new[]
				{
					agentData.Assignment, agentData8To91.Assignment, agentData8To92.Assignment, agentData8To93.Assignment,agentData9To101.Assignment, agentData9To102.Assignment, agentData9To103.Assignment, agentData10To111.Assignment, agentData10To112.Assignment
				});
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(10));
		}

		[Test, Ignore("40939")]
		public void ShouldDoNothingWhenNoMaxSeatLimit()
		{
			var site = new Site("_").WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour1.Assignment });
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, scenario, optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(0);
		}

		protected abstract OptimizationPreferences CreateOptimizationPreferences();
	}
}