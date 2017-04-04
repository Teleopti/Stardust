using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class DescriptionExtensions
	{
		public static Team WithDescription(this Team team, Description description)
		{
			team.SetDescription(description);
			return team;
		}
	}

	public static class PersonExtensions
	{
		public static IPerson WithName(this IPerson person, Name name)
		{
			person.SetName(name);
			return person;
		}

		public static Person WithName(this Person person, Name name)
		{
			person.SetName(name);
			return person;
		}

		public static Person InTimeZone(this Person agent, TimeZoneInfo timeZoneInfo)
		{
			agent.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, params ISkill[] skills)
		{
			return agent.WithPersonPeriod((IRuleSetBag)null, null, null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, ITeam team, params ISkill[] skills)
		{
			return agent.WithPersonPeriod((IRuleSetBag)null, null, team, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(ruleSet, null, null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, ITeam team, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(ruleSet, null, team, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IContract contract, params ISkill[] skills)
		{
			return agent.WithPersonPeriod((IRuleSetBag)null, contract, null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, IContract contract, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(ruleSet, contract, null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, IContract contract, ITeam team, params ISkill[] skills)
		{
			return ruleSet == null ? 
				agent.WithPersonPeriod((IRuleSetBag)null, contract, team, skills) : 
				agent.WithPersonPeriod(new RuleSetBag(ruleSet), contract, team, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IRuleSetBag ruleSetBag, IContract contract, ITeam team, params ISkill[] skills)
		{
			var personPeriod = new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") });
			if (skills.Any())
			{
				foreach (var skill in skills)
				{
					personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
				}
			}
			if (ruleSetBag != null)
			{
				personPeriod.RuleSetBag = ruleSetBag;
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
			return agent;
		}

		public static Person WithSchedulePeriodOneDay(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
			return agent;
		}

		public static Person WithSchedulePeriodTwoDays(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 2));
			return agent;
		}

		public static Person WithSchedulePeriodOneWeek(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 1));
			return agent;
		}

		public static Person WithSchedulePeriodOneMonth(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Month, 1));
			return agent;
		}
	}
}