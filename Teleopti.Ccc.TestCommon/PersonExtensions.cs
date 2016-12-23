using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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

		public static Person KnowsSkill(this Person agent, params ISkill[] skills)
		{
			if (skills.Any())
			{
				agent.AddPeriodWithSkills(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skills);
			}
			else
			{
				agent.AddPersonPeriod(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			}
			return agent;
		}

		public static Person WithPersonPeriod(this Person agent, DateOnly date, IWorkShiftRuleSet ruleSet, params ISkill[] skills )
		{
			agent.AddPeriodWithSkills(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skills);
			if (ruleSet != null)
			{
				agent.Period(date).RuleSetBag = new RuleSetBag(ruleSet);
			}
			return agent;
		}
	}
}