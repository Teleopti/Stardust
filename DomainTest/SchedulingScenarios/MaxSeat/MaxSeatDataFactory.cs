using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	public static class MaxSeatDataFactory
	{
		public static MaxSeatData CreateAgentWithAssignment(DateOnly date, ISite site, IRuleSetBag ruleSetBag, IScenario scenario, IActivity activity, TimePeriod assignmentPeriod)
		{
			var team = new Team { Site = site };
			return CreateAgentWithAssignment(date, team, ruleSetBag, scenario, activity, assignmentPeriod);
		}

		public static MaxSeatData CreateAgentWithAssignment(DateOnly date, ITeam team, IRuleSetBag ruleSetBag, IScenario scenario, IActivity activity, TimePeriod assignmentPeriod)
		{
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Day, 1);
			agent.AddPersonPeriod(new PersonPeriod(date.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = ruleSetBag });
			agent.AddSchedulePeriod(schedulePeriod);
			var assignment = new PersonAssignment(agent, scenario, date);
			assignment.AddActivity(activity, assignmentPeriod);
			assignment.SetShiftCategory(new ShiftCategory("_").WithId());
			return new MaxSeatData(agent, assignment);
		}
	}
}