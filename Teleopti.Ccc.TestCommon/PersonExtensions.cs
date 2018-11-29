using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.TestCommon
{
	public static class DescriptionExtensions
	{
		public static Team WithDescription(this Team team, Description description)
		{
			team.SetDescription(description);
			return team;
		}

		public static Team WithDescription(this Team team, string description)
		{
			team.SetDescription(new Description(description));
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
			return agent.WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IWorkShiftRuleSet ruleSet, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(periodStart, new RuleSetBag(ruleSet), new ContractWithMaximumTolerance(), null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IRuleSetBag ruleSetBag, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(ruleSetBag, new ContractWithMaximumTolerance(), null, skills);
		}
		
		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IRuleSetBag ruleSetBag, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(periodStart, ruleSetBag, new ContractWithMaximumTolerance(), null, skills);
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
		
		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, IContract contract, IContractSchedule contractSchedule)
		{
			return agent.WithPersonPeriod(new RuleSetBag(ruleSet), contract, contractSchedule, null,null);
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, IContract contract, ITeam team, params ISkill[] skills)
		{
			return WithPersonPeriod(agent, DateOnly.MinValue, ruleSet, contract, team, skills);
		}
		
		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IWorkShiftRuleSet ruleSet, IContract contract, ITeam team, params ISkill[] skills)
		{
			return ruleSet == null ? 
				agent.WithPersonPeriod(periodStart, (IRuleSetBag)null, contract, team, skills) : 
				agent.WithPersonPeriod(periodStart, new RuleSetBag(ruleSet){Description = new Description("_")}, contract, team, skills);
		}
		
		public static Person WithPersonPeriod(this Person agent, IRuleSetBag ruleSetBag, IContract contract, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(ruleSetBag, contract, null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IWorkShiftRuleSet ruleSet, IContractSchedule contractSchedule, params ISkill[] skills)
		{
			return WithPersonPeriod(agent, DateOnly.MinValue, ruleSet, contractSchedule, skills);
		}
		
		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IWorkShiftRuleSet ruleSet, IContractSchedule contractSchedule, params ISkill[] skills)
		{
			var team = new Team {Site = new Site("_")};
			team.SetDescription(new Description("_"));
			return WithPersonPeriod(agent, periodStart, ruleSet, contractSchedule, team, skills);
		}
		
		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IWorkShiftRuleSet ruleSet, IContractSchedule contractSchedule, ITeam team, params ISkill[] skills)
		{
			var newAgent = agent.WithPersonPeriod(periodStart, ruleSet, (IContract)null, team, skills);
			newAgent.Period(periodStart).PersonContract.ContractSchedule = contractSchedule;
			return newAgent;
		}
		
		public static Person WithPersonPeriod(this Person agent, IContractSchedule contractSchedule, params ISkill[] skills)
		{
			var team = new Team {Site = new Site("_")};
			team.SetDescription(new Description("_"));
			var newAgent = agent.WithPersonPeriod(null, team, skills);
			newAgent.Period(DateOnly.MinValue).PersonContract.ContractSchedule = contractSchedule;
			return newAgent;
		}

		public static Person WithPersonPeriod(this Person agent, IRuleSetBag ruleSetBag, IContract contract, ITeam team, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(DateOnly.MinValue, ruleSetBag, contract, team, skills);
		}
		
		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IRuleSetBag ruleSetBag, IContract contract, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(periodStart, ruleSetBag, contract, null, skills);
		}

		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IRuleSetBag ruleSetBag, IContract contract, ITeam team, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(periodStart, ruleSetBag, contract, null, null, team, skills);
		}

		public static Person WithPersonPeriod(this Person agent, IRuleSetBag ruleSetBag, IContract contract,
			IContractSchedule contractSchedule, IPartTimePercentage partTimePercentage, ITeam team, params ISkill[] skills)
		{
			return agent.WithPersonPeriod(DateOnly.MinValue, ruleSetBag, contract, contractSchedule, partTimePercentage, team, skills);
		}
		
		public static Person WithPersonPeriod(this Person agent, DateOnly periodStart, IRuleSetBag ruleSetBag, IContract contract, 
			IContractSchedule contractSchedule, IPartTimePercentage partTimePercentage, ITeam team, params ISkill[] skills)
		{
			var defaultTeam = new Team {Site = new Site("_")};
			defaultTeam.SetDescription(new Description("_"));
			var personPeriod = new PersonPeriod(periodStart, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), defaultTeam);
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
			if (contractSchedule != null)
			{
				personPeriod.PersonContract.ContractSchedule = contractSchedule;
			}
			if (partTimePercentage != null)
			{
				personPeriod.PersonContract.PartTimePercentage = partTimePercentage;
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
		
		public static Person WithSchedulePeriodTwoWeeks(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 2));
			return agent;
		}

		public static Person WithSchedulePeriodOneWeek(this Person agent, DateOnly date, int numberOfDayOffs)
		{
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			schedulePeriod.SetDaysOff(numberOfDayOffs);
			agent.AddSchedulePeriod(schedulePeriod);
			return agent;
		}

		public static Person WithSchedulePeriodOneMonth(this Person agent, DateOnly date)
		{
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Month, 1));
			return agent;
		}
	}
}