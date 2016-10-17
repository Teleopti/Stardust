using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MaxSeat;
using Teleopti.Ccc.Domain.Scheduling;
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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {agentData.Agent, agentScheduledForAnHourData.Agent}, schedules, scenario);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment(true).Period.StartDateTime.TimeOfDay
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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Agent, agentData.Agent, agent2Data.Agent}, schedules, scenario);

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
		
			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHourData.Agent, agentData.Agent }, schedules, scenario);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment(true).Period.StartDateTime.TimeOfDay
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

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {  agentData.Agent }, schedules, scenario);

			schedules[agentData.Agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.First().Payload.RequiresSeat.Should().Be.False();
		}

		[Test, Ignore("#40939")]
		public void ShouldOnlyOptimizeAgentsHavingTheMaxSeatSkillThatIsOverLimit()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var siteOverLimit = new Site("_") { MaxSeats = 1 }.WithId();
			var siteUnderLimit = new Site("_") {MaxSeats =  10}.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var bag = new RuleSetBag(ruleSet);
			var agentDataSiteOverLimit = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, siteOverLimit, bag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentDataSiteUnderLimit = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, siteUnderLimit, bag, scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, siteOverLimit, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentDataSiteOverLimit.Assignment, agentDataSiteUnderLimit.Assignment, agentScheduledForAnHourData.Assignment });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentDataSiteUnderLimit.Agent, agentDataSiteOverLimit.Agent, agentScheduledForAnHourData.Agent  }, schedules, scenario);

			schedules[agentDataSiteUnderLimit.Agent].ScheduledDay(dateOnly).PersonAssignment(true).Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}
	}	
}