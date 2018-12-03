using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class PersonPeriodFactory
    {
		
		public static PersonPeriod CreatePersonPeriodFromDateTime(DateTime startDate, IPersonContract personContract, ITeam team)
		{
			return new PersonPeriod(new DateOnly(startDate), personContract, team);
		}

        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate)
        {
            IPersonContract personContract =
                new PersonContract(new Contract("my first contract"), new PartTimePercentage("Testing"),
                                   new ContractSchedule("Test1"));
	        ITeam team = new Team();
            IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);

            return personPeriod;
        }

        public static IPersonPeriod CreatePersonPeriodWithSkills(DateOnly startDate, params ISkill[] skills)
        {
            IPersonContract personContract =
                new PersonContract(new Contract("my contract"), new PartTimePercentage("Testing"),
                                   new ContractSchedule("Test1"));

            ITeam team = new Team();
            IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);

            foreach (Skill skill in skills)
            {
                Percent percent = new Percent(1);
                IPersonSkill personSkill = new PersonSkill(skill, percent);
                ((IPersonPeriodModifySkills)personPeriod).AddPersonSkill(personSkill);
            }

            return personPeriod;
        }

		public static IPersonPeriod CreatePersonPeriodWithSkillsWithSite(DateOnly startDate, params ISkill[] skills)
		{
			IPersonContract personContract =
				 new PersonContract(new Contract("my contract"), new PartTimePercentage("Testing"),
										  new ContractSchedule("Test1"));

			ITeam team = new Team();
			team.Site = new Site("Site1");
			IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);

			foreach (Skill skill in skills)
			{
				Percent percent = new Percent(1);
				IPersonSkill personSkill = new PersonSkill(skill, percent);
				((IPersonPeriodModifySkills)personPeriod).AddPersonSkill(personSkill);
			}

			return personPeriod;
		}

		public static IPersonPeriod CreatePersonPeriodWithSkills(DateOnly startDate, IRuleSetBag ruleSetBag,
                                                                 params ISkill[] skills)
        {
            IPersonPeriod personPeriod = CreatePersonPeriodWithSkills(startDate, skills);
            personPeriod.RuleSetBag = ruleSetBag;

            return personPeriod;
        }

        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate, ITeam team)
        {
            IPersonContract personContract =
                new PersonContract(new Contract("my first contract"), new PartTimePercentage("Testing"),
                                   new ContractSchedule("Test1"));

            IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);

            return personPeriod;
        }

        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate, ITeam team, IRuleSetBag ruleSetBag)
        {
            IPersonPeriod personPeriod = CreatePersonPeriod(startDate, team);
            personPeriod.RuleSetBag = ruleSetBag;

            return personPeriod;
        }

        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate, ITeam team, IBudgetGroup budgetGroup)
        {
            IPersonPeriod personPeriod = CreatePersonPeriod(startDate, team);
            personPeriod.BudgetGroup = budgetGroup;

            return personPeriod;
        }

        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate, IPersonContract personContract, ITeam team)
        {
            return new PersonPeriod(startDate, personContract, team);
        }
    }
}
