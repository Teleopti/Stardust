using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class PersonExtensions
	{
		public static Person InTimeZone(this Person agent, TimeZoneInfo timeZoneInfo)
		{
			agent.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, params ISkill[] skills)
		{
			agent.addPeriod(null, null, null, skills);
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, ITeam team, params ISkill[] skills)
		{
			agent.addPeriod(null, null, team, skills);
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, params ISkill[] skills)
		{
			agent.addPeriod(ruleSet, null, null, skills);
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, ITeam team, params ISkill[] skills)
		{
			agent.addPeriod(ruleSet, null, team, skills);
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, IContract contract, params ISkill[] skills)
		{
			agent.addPeriod(null, contract, null, skills);
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, IContract contract, params ISkill[] skills)
		{
			agent.addPeriod(ruleSet, contract, null, skills);
			return agent;
		}

		private static void addPeriod(this Person agent, IWorkShiftRuleSet ruleSet, IContract contract, ITeam team, params ISkill[] skills)
		{
			var personPeriod = new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") });
			if (skills.Any())
			{
				foreach (var skill in skills)
				{
					personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
				}
			}
			if (ruleSet != null)
			{
				personPeriod.RuleSetBag = new RuleSetBag(ruleSet);
			}
			if (contract != null)
			{
				personPeriod.PersonContract = new PersonContract(contract, new PartTimePercentage("_"), new ContractSchedule("_"));
			}
			if (team != null)
			{
				personPeriod.Team = team;
			}
			agent.AddPersonPeriod(personPeriod);
		}

		public static Person WithSchedulePeriodOneDay(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
			return agent;
		}

		public static Person WithSchedulePeriodOneWeek(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			return agent;
		}
	}
}