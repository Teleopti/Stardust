using System;
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
			var activity = new Activity("_") {RequiresSeat = true};
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), shiftCategory));
			var site = new Site("_")
			{
				MaxSeats = 1
			};
			var team = new Team {Site = site};
			var agentScheduledForAnHour = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var assigmentHour = new PersonAssignment(agentScheduledForAnHour, scenario, dateOnly);
			assigmentHour.AddActivity(activity, new TimePeriod(8, 0, 9, 0)); //should force other agent to start 9
			assigmentHour.SetShiftCategory(shiftCategory);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			var personPeriod = new PersonPeriod(dateOnly.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) };
			agent.AddPersonPeriod(personPeriod);
			agent.AddSchedulePeriod(schedulePeriod);
			var assignmentToBeMoved = new PersonAssignment(agent, scenario, dateOnly);
			assignmentToBeMoved.AddActivity(activity, new TimePeriod(8, 0, 16, 0)); 
			assignmentToBeMoved.SetShiftCategory(shiftCategory);
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] {assigmentHour, assignmentToBeMoved});

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] {agentScheduledForAnHour, agent}, schedules, scenario);

			schedules[agent].ScheduledDay(dateOnly).PersonAssignment(true).Period.StartDateTime.TimeOfDay
				.Should().Be.EqualTo(TimeSpan.FromHours(9));
		}
	}
}