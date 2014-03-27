﻿using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class PersonPeriodFactory
    {
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

            IActivity activity = new Activity("dummy activity");

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
            IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);
            IActivity activity = new Activity("dummy activity");
            return personPeriod;
        }
    }
}
