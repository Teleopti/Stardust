using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
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

		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeats)]
		public void HappyPath(MaxSeatsFeatureOptions maxSeatsFeatureOptions)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = maxSeatsFeatureOptions;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderOpenHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = true}.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay}, CreateOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderAgentTimeZoneWhenCheckingOpenHours()
		{ 
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = true }.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((Person) agentData.Agent).InTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, CreateOptimizationPreferences());

			var time = TimeZoneHelper.ConvertFromUtc(schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime, 
													TimeZoneInfoFactory.StockholmTimeZoneInfo());


			time.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderClosedHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = true}.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(0, 7), new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, CreateOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderIntersectingOpenHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(0, 8), new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 7, 0, 60), new TimePeriodWithSegment(15, 0, 15, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, CreateOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(7));
		}

		[Test]
		public void ShouldConsiderRequiresSkillWhenCheckingOpenHours()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true, RequiresSkill = false}.WithId();
			var skillActivity = new Activity("_") { RequiresSeat = true}.WithId();
			var skill = new Skill("_", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = skillActivity };
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 24));
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(8), OperatorLimiter.Equals));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			((PersonPeriod)agentData.Agent.Period(dateOnly)).AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, new[] { skillDay }, CreateOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(7));
		}

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().ShiftCategory.Equals(shiftCategory))
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotRollbackEveryChangeIfLastOneBreakRule()
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
			var agentThatWillBeOptimized = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentWillBreakShiftCategory = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var shiftCategoryLimitation = new ShiftCategoryLimitation(shiftCategory) { MaxNumberOf = 0 };
			agentWillBreakShiftCategory.Agent.SchedulePeriod(dateOnly).AddShiftCategoryLimitation(shiftCategoryLimitation);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentThatWillBeOptimized.Assignment, agentWillBreakShiftCategory.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.UseShiftCategoryLimitations = true;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentThatWillBeOptimized.Agent, agentWillBreakShiftCategory.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSetCorrectTag()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.General.ScheduleTag = new ScheduleTag();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly).ScheduleTag()
				.Should().Be.SameInstanceAs(optPreferences.General.ScheduleTag);
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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldDoNothingWhenMaxSeatGetsEqual()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(10, 0, 10, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentDataOneHour2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour1.Assignment, agentDataOneHour2.Assignment });
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

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
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(10));
		}

		[Test]
		public void ShouldDoNothingWhenNoMaxSeatLimit()
		{
			var site = new Site("_").WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment });
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(0);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldConsiderActivityRequireSeat(bool ruleSetOrder)
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var activityRequireNoSeat = new Activity("_") { RequiresSeat = false }.WithId();
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), CreateOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftLayers.First().Payload.RequiresSeat.Should().Be.False();
		}

		[Test]
		public void ShouldDoNothingWhenNotUsingMaxSeatLimit()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Assignment, agentData.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.DoNotConsiderMaxSeats;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotOptimizeAgentNotHavingTheMaxSeatSkillThatIsOverLimit()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var siteOverLimit = new Site("_") { MaxSeats = 1 }.WithId();
			var teamOverLimit = new Team { Description = new Description("_"), Site = siteOverLimit };
			var siteUnderLimit = new Site("_") { MaxSeats = 10 }.WithId();
			var teamUnderLimit = new Team { Description = new Description("_"), Site = siteUnderLimit };
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
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentDataSiteUnderLimit.Agent, agentScheduledForAnHourData1.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentDataSiteUnderLimit.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldNotCareAboutNotInvolvedMaxSeatSkill()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var siteOverLimit = new Site("_") { MaxSeats = 1 }.WithId();
			var teamOverLimit = new Team { Description = new Description("_"), Site = siteOverLimit };
			var siteUnderLimit = new Site("_") { MaxSeats = 10 }.WithId();
			var teamUnderLimit = new Team { Description = new Description("_"), Site = siteUnderLimit };
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
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentDataOverLimit.Agent, agentScheduledForAnHourData1.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentDataOverLimit.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}


		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak, ExpectedResult = false)]
		[TestCase(MaxSeatsFeatureOptions.ConsiderMaxSeats, ExpectedResult = true)]
		public bool ShouldNotBreakMaxSeat(MaxSeatsFeatureOptions maxSeatsFeatureOptions)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = maxSeatsFeatureOptions;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			return schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Any();
		}

		[Test]
		public void ShouldOnlyRemoveScheduleAffectingOverMaxSeatLimit()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 22, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentData3.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent, agentData3.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData3.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldTryToFixMaxSeatForAllBeforeRemovingShifts()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team1 = new Team { Description = new Description("_"), Site = site };
			var team2 = new Team { Description = new Description("_"), Site = site };

			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team1));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(16, 0, 16, 0, 60), new TimePeriodWithSegment(24, 0, 24, 0, 60), new ShiftCategory("_").WithId()));

			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team2, new RuleSetBag(ruleSet2), scenario, activity, new TimePeriod(8, 0, 16, 0));

			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent}, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData1.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotCrashWhenOptimizeEmptyDays()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team1 = new Team { Description = new Description("_"), Site = site };

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
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent}, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);
		}

		[Test]
		public void ShouldNotCrashWhenShiftLayerDoesNotMatchSkillStaffPeriod()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 10, 8, 10, 60), new TimePeriodWithSegment(16, 10, 16, 10, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentDataOneHour.Assignment });
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void PeakShouldNotBe0ForNonSkillIntervals(bool ruleSet1First)
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
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
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldNotOptimizeEmptyDays()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team1 = new Team { Description = new Description("_"), Site = site };

			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team1));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet1 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSet2 = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(16, 0, 16, 0, 60), new TimePeriodWithSegment(24, 0, 24, 0, 60), new ShiftCategory("_").WithId()));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet1), scenario, activity, new TimePeriod(8, 0, 16, 0));
			agentData1.Assignment.Clear();
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet2), scenario, activity, new TimePeriod(16, 0, 24, 0));
			var agentData3 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team1, new RuleSetBag(ruleSet2), scenario, activity, new TimePeriod(16, 0, 24, 0));

			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentData3.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent, agentData3.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentData1.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotMoveMoreSchedulesThanNecessary()
		{
			if (GetType() == typeof(MaxSeatOptimizationTeamTest))
				Assert.Ignore("Won't probably be fixed for first version. When we know more fix or delete (=move this test to MaxSeatOptimizationBlockTest)");

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
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agent2Data.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotRemoveMoreThanNecessary()
		{
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentDataOneHour.Assignment });
			var optPreferences = CreateOptimizationPreferences();
			optPreferences.Advanced.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules.SchedulesForDay(dateOnly).Count(x => x.IsScheduled()).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotChangeDayWhereMaxSeatIsOkWhenMultipleDaysSelected()
		{
			if (GetType() == typeof(MaxSeatOptimizationBlockTest))
				Assert.Ignore("Won't probably be fixed for first version. When we know more fix or delete (=move this test to MaxSeatOptimizationTeamTest)");
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(period.EndDate, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDatas = MaxSeatDataFactory.CreateAgentWithAssignments(period, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));

			var assignments = new List<IPersonAssignment>();
			foreach (var maxSeatData in agentDatas)
			{
				assignments.Add(maxSeatData.Assignment);
			}
			assignments.Add(agentDataOneHour.Assignment);

			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, assignments);
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(period, new[] { agentDatas.First().Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentDatas.First().Agent].ScheduledDay(period.StartDate)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldFixMaxSeatIfIssueIsNotOnFirstDay()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(period.EndDate, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(16, 0, 17, 0));
			var agentDatas = MaxSeatDataFactory.CreateAgentWithAssignments(period, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var assignments = new List<IPersonAssignment>();
			foreach (var maxSeatData in agentDatas)
			{
				assignments.Add(maxSeatData.Assignment);
			}
			assignments.Add(agentDataOneHour.Assignment);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, period, assignments);
			var optPreferences = CreateOptimizationPreferences();

			Target.Optimize(period, new[] { agentDatas.First().Agent }, schedules, Enumerable.Empty<ISkillDay>(), optPreferences);

			schedules[agentDatas.First().Agent].ScheduledDay(period.EndDate)
				.PersonAssignment()
				.Period.StartDateTime.TimeOfDay.Should()
				.Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test, Ignore("To be fixed")]
		public void ShouldCheckMaxPeaksOnNightShifts()
		{
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site };
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = new DateOnly(2016, 10, 25);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(18, 0, 18, 0, 60), new TimePeriodWithSegment(26, 0, 26, 0, 60), new ShiftCategory("_").WithId()));
			var agentDataOneHour = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(10, 0, 11, 0));
			var agentDataOneHourAtNight = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly.AddDays(1), team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(0, 0, 1, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(9, 0, 17, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new[] { agentData.Assignment, agentDataOneHour.Assignment, agentDataOneHourAtNight.Assignment });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent }, schedules, Enumerable.Empty<ISkillDay>(), CreateOptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		protected abstract OptimizationPreferences CreateOptimizationPreferences();
	}
}