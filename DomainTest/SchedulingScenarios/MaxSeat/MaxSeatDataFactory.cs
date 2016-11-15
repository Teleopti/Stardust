﻿using System;
using System.Collections.Generic;
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
		public static MaxSeatData CreateAgentWithAssignment(DateOnly date, ITeam team, IRuleSetBag ruleSetBag, IScenario scenario, IActivity activity, TimePeriod assignmentPeriod)
		{
			return CreateAgentWithAssignment(date, team, ruleSetBag, scenario, activity, assignmentPeriod, TimeZoneInfo.Utc);
		}

		public static MaxSeatData CreateAgentWithAssignment(DateOnly date, ITeam team, IRuleSetBag ruleSetBag, IScenario scenario, IActivity activity, TimePeriod assignmentPeriod, TimeZoneInfo timeZoneInfo)
		{
			var agent = new Person().WithId().InTimeZone(timeZoneInfo);
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Day, 1);
			agent.AddPersonPeriod(new PersonPeriod(date.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = ruleSetBag });
			agent.AddSchedulePeriod(schedulePeriod);
			var assignment = new PersonAssignment(agent, scenario, date);
			assignment.AddActivity(activity, assignmentPeriod);
			assignment.SetShiftCategory(new ShiftCategory("_").WithId());
			return new MaxSeatData(agent, assignment);
		}

		public static IList<MaxSeatData> CreateAgentWithAssignments(DateOnlyPeriod period, ITeam team, IRuleSetBag ruleSetBag, IScenario scenario, IActivity activity, TimePeriod assignmentPeriod)
		{
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var schedulePeriod = new SchedulePeriod(period.StartDate, SchedulePeriodType.Day, period.DayCount());
			agent.AddPersonPeriod(new PersonPeriod(period.StartDate.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = ruleSetBag });
			agent.AddSchedulePeriod(schedulePeriod);

			var data = new List<MaxSeatData>();

			foreach (var date in period.DayCollection())
			{
				var assignment = new PersonAssignment(agent, scenario, date);
				assignment.AddActivity(activity, assignmentPeriod);
				assignment.SetShiftCategory(new ShiftCategory("_").WithId());
				data.Add(new MaxSeatData(agent, assignment));
			}

			return data;
		}
	}
}