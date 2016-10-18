using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	public class MaxSeatOptimizationTest
	{
		public MaxSeatOptimization Target;

		[Test]
		public void ShouldMoveShiftAwayFromMaxSeatPeak()
		{
			var activity = new Activity("_") {RequiresSeat = true}.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] {agentData.Assignment, agentScheduledForAnHourData.Assignment});

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {agentData.Agent, agentScheduledForAnHourData.Agent}, schedules, scenario, new OptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldNotMoveMoreSchedulesThanNecessary()
		{
			var activity = new Activity("_") {RequiresSeat = true}.WithId();
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agent2Data = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Assignment, agentData.Assignment, agent2Data.Assignment});

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Agent, agentData.Agent, agent2Data.Agent}, schedules, scenario, new OptimizationPreferences());

			schedules.SchedulesForPeriod(dateOnly.ToDateOnlyPeriod(), agentData.Agent, agent2Data.Agent).Select(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay)
				.Should().Have.SameValuesAs(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldDoNothingWhenNotAboveMaxSeatLimitation()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Assignment, agentData.Assignment });
		
			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Agent, agentData.Agent }, schedules, scenario, new OptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderActivityRequireSeat()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var activityRequireNoSeat = new Activity("_") {RequiresSeat = false}.WithId();
			var site = new Site("_") { MaxSeats = 0 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSetNotRequireSeat = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityRequireNoSeat, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag(ruleSetNotRequireSeat);
			ruleSetBag.AddRuleSet(ruleSet);
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, ruleSetBag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {  agentData.Agent }, schedules, scenario, new OptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftLayers.First().Payload.RequiresSeat.Should().Be.False();
		}

		[Test]
		public void ShouldOnlyOptimizeAgentsHavingTheMaxSeatSkillThatIsOverLimit()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var siteOverLimit = new Site("_") { MaxSeats = 1 }.WithId();
			var siteUnderLimit = new Site("_") {MaxSeats =  10}.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var bag = new RuleSetBag(ruleSet);
			var agentDataSiteOverLimit = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, siteOverLimit, bag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentDataSiteUnderLimit = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, siteUnderLimit, bag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, siteOverLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentDataSiteOverLimit.Assignment, agentDataSiteUnderLimit.Assignment, agentScheduledForAnHourData.Assignment });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentDataSiteUnderLimit.Agent, agentDataSiteOverLimit.Agent, agentScheduledForAnHourData.Agent  }, schedules, scenario, new OptimizationPreferences());

			schedules[agentDataSiteUnderLimit.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldBeAbleToUseLongerShiftIfContractTimeIsTheSame()
		{
			var activity = new Activity("_") { RequiresSeat = true, InContractTime = true}.WithId();
			var nonContractTimeActivity = new Activity("_") { RequiresSeat = true, InContractTime = false}.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			// 1 shift -> contract time 9-17, non contract time 17-18
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(nonContractTimeActivity, new TimePeriodWithSegment(1, 0, 1, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60)));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, new OptimizationPreferences());

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.EndDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(18));
		}

		[Test]
		public void ShouldConsiderKeepStartTime()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] {agentData.Assignment, agentScheduledForAnHourData.Assignment});
			var optPrefs = new OptimizationPreferences {Shifts = {KeepStartTimes = true}};

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, optPrefs);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderKeepEndTime()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPrefs = new OptimizationPreferences { Shifts = { KeepEndTimes = true } };

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, optPrefs);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.EndDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(16));
		}

		[Test]
		public void ShouldConsiderAlterBetween()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPrefs = new OptimizationPreferences { Shifts = { AlterBetween = true, SelectedTimePeriod = new TimePeriod(10, 0, 14, 0)} };

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, optPrefs);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderDoNotMoveActivity()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPrefs = new OptimizationPreferences { Shifts = { SelectedActivities = new List<IActivity> {activity} }};

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, optPrefs);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderLengthOfActivity()
		{
			var activity = new Activity("_") { RequiresSeat = true, InContractTime = true }.WithId();
			var nonContractTimeActivity = new Activity("_") { RequiresSeat = true, InContractTime = false }.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			// 1 shift -> contract time 9-17, non contract time 17-18
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(18, 0, 18, 0, 60), new ShiftCategory("_").WithId()));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(nonContractTimeActivity, new TimePeriodWithSegment(1, 0, 1, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60)));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			agentData.Assignment.AddActivity(nonContractTimeActivity, new DateTimePeriod(dateOnly.Year, dateOnly.Month, dateOnly.Day, 16, dateOnly.Year, dateOnly.Month, dateOnly.Day, 19));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPrefs = new OptimizationPreferences { Shifts = { KeepActivityLength = true, ActivityToKeepLengthOn = nonContractTimeActivity}};

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, optPrefs);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.EndDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(19));
		}

		[Test]
		public void ShouldConsiderKeepShiftCategory()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var oldShiftCategory = agentData.Assignment.ShiftCategory;
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPrefs = new OptimizationPreferences { Shifts = { KeepShiftCategories = true} };

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, optPrefs);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().ShiftCategory
				.Should().Be.SameInstanceAs(oldShiftCategory);
		}

		[Test, Ignore("40939")]
		public void ShouldConsiderKeepShifts()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var site = new Site("_") { MaxSeats = 1 }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, site, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData.Assignment, agentScheduledForAnHourData.Assignment });
			var optPrefs = new OptimizationPreferences { Shifts = {KeepShifts = true, KeepShiftsValue = 1d} };

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData.Agent, agentScheduledForAnHourData.Agent }, schedules, scenario, optPrefs);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}
	}	
}