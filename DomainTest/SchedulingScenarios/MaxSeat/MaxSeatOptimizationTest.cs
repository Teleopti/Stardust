﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MaxSeat;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), shiftCategory));
			var site = new Site("_")
			{
				MaxSeats = 1
			}.WithId();
			var team = new Team {Site = site};
			var agentScheduledForAnHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var assigmentHour = new PersonAssignment(agentScheduledForAnHour, scenario, dateOnly);
			assigmentHour.AddActivity(activity, new TimePeriod(8, 0, 9, 0)); //should force other agent to start 9
			assigmentHour.SetShiftCategory(shiftCategory);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			agent.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) });
			agentScheduledForAnHour.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) });
			agent.AddSchedulePeriod(schedulePeriod);
			var assignmentToBeMoved = new PersonAssignment(agent, scenario, dateOnly);
			assignmentToBeMoved.AddActivity(activity, new TimePeriod(8, 0, 16, 0)); 
			assignmentToBeMoved.SetShiftCategory(shiftCategory);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] {assigmentHour, assignmentToBeMoved});

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {agentScheduledForAnHour, agent}, schedules, scenario);

			schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldNotMoveMoreSchedulesThanNecessary()
		{
			var activity = new Activity("_") {RequiresSeat = true}.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), shiftCategory));
			var site = new Site("_")
			{
				MaxSeats = 2
			}.WithId();
			var team = new Team {Site = site};

			var agentScheduledForAnHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var assigmentHour = new PersonAssignment(agentScheduledForAnHour, scenario, dateOnly);
			assigmentHour.AddActivity(activity, new TimePeriod(8, 0, 9, 0)); 
			assigmentHour.SetShiftCategory(shiftCategory);
			agentScheduledForAnHour.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) });

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			agent.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) });
			agent.AddSchedulePeriod(schedulePeriod);
			var assignmentToBeMoved = new PersonAssignment(agent, scenario, dateOnly);
			assignmentToBeMoved.AddActivity(activity, new TimePeriod(8, 0, 16, 0)); 
			assignmentToBeMoved.SetShiftCategory(shiftCategory);

			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod2 = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			agent2.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) });
			agent2.AddSchedulePeriod(schedulePeriod2);
			var assignmentToBeMoved2 = new PersonAssignment(agent2, scenario, dateOnly);
			assignmentToBeMoved2.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			assignmentToBeMoved2.SetShiftCategory(shiftCategory);


			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] {assigmentHour, assignmentToBeMoved, assignmentToBeMoved2 });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {agentScheduledForAnHour, agent, agent2}, schedules, scenario);

			var startTimes = schedules.SchedulesForPeriod(dateOnly.ToDateOnlyPeriod(), agent, agent2).Select(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay);
			startTimes.Should().Have.SameValuesAs(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
		}

		[Test]
		public void ShouldDoNothingWhenNotAboveMaxSeatLimitation()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), shiftCategory));
			var site = new Site("_")
			{
				MaxSeats = 2
			}.WithId();
			var team = new Team { Site = site };
			var agentScheduledForAnHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var assigmentHour = new PersonAssignment(agentScheduledForAnHour, scenario, dateOnly);
			assigmentHour.AddActivity(activity, new TimePeriod(8, 0, 9, 0)); //should not break max seat
			assigmentHour.SetShiftCategory(shiftCategory);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			agent.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) });
			agentScheduledForAnHour.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) });
			agent.AddSchedulePeriod(schedulePeriod);
			var assignmentToBeMoved = new PersonAssignment(agent, scenario, dateOnly);
			assignmentToBeMoved.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			assignmentToBeMoved.SetShiftCategory(shiftCategory);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { assigmentHour, assignmentToBeMoved });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentScheduledForAnHour, agent }, schedules, scenario);

			schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldConsiderActivityRequireSeat()
		{
			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var activityRequireNoSeat = new Activity("_") {RequiresSeat = false}.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var ruleSetNotRequireSeat = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityRequireNoSeat, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), shiftCategory));
			var site = new Site("_")
			{
				MaxSeats = 0
			}.WithId();
			var team = new Team { Site = site };
			
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			var ruleSetBag = new RuleSetBag(ruleSetNotRequireSeat);
			ruleSetBag.AddRuleSet(ruleSet);
			agent.AddPersonPeriod(new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = ruleSetBag });
			agent.AddSchedulePeriod(schedulePeriod);
			var assignmentToBeMoved = new PersonAssignment(agent, scenario, dateOnly);
			assignmentToBeMoved.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
			assignmentToBeMoved.SetShiftCategory(shiftCategory);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { assignmentToBeMoved });

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {  agent }, schedules, scenario);

			schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).ShiftLayers.First().Payload.RequiresSeat.Should().Be.False();
		}
	}	
}