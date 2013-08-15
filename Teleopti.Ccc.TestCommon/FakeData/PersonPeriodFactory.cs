﻿using System.Drawing;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: sumeda herath
    /// Created date: 2008-01-23
    /// </remarks>
    public static class  PersonPeriodFactory
    {
        
        /// <summary>
        /// Creates the person period.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate)
        {
            IPersonContract personContract =
               new PersonContract(new Contract("my first contract"),new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
            ITeam team = new Team();
            //team.WriteProtection = 10000;
            IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);
          
            return personPeriod;
        }

        /// <summary>
        /// Creates the person period.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="skills">The skills.</param>
        /// <returns></returns>
        public static IPersonPeriod CreatePersonPeriodWithSkills(DateOnly startDate, params ISkill[] skills)
        {
            IPersonContract personContract =
               new PersonContract(new Contract("my contract"), new PartTimePercentage("Testing"), new ContractSchedule("Test1"));

            ITeam team = new Team();
            //team.WriteProtection = 10000;
            IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract,team);

            IGroupingActivity groupActivity = new GroupingActivity("dummy group activity");
            IActivity activity = new Activity("dummy activity");
            activity.GroupingActivity = groupActivity;

            foreach (Skill skill in skills)
            {
                Percent percent = new Percent(1);
                IPersonSkill personSkill = new PersonSkill(skill, percent);
                ((IPersonPeriodModifySkills)personPeriod).AddPersonSkill(personSkill);
            }

            return personPeriod;
        }

        public static IPersonPeriod CreatePersonPeriodWithSkills(DateOnly startDate, IRuleSetBag ruleSetBag, params ISkill[] skills)
        {
            IPersonPeriod personPeriod = CreatePersonPeriodWithSkills(startDate, skills);
            personPeriod.RuleSetBag = ruleSetBag;

            return personPeriod;
        }


        /// <summary>
        /// Creates the person period.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static PersonPeriod CreatePersonPeriodWithSkills(DateOnly startDate, Team team)
        {
            PersonContract personContract =
               new PersonContract(new Contract("my first contract"),new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
          
            PersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);

            GroupingActivity groupActivity = new GroupingActivity("dummy group activity");
            Activity activity = new Activity("dummy activity");
            activity.GroupingActivity = groupActivity;

            Skill skill = new Skill("test skill", "test", Color.Red, 15, SkillTypeFactory.CreateSkillType());
            skill.Activity = activity;
            Percent percent = new Percent(1);
            PersonSkill personSkill = new PersonSkill(skill, percent);

            personPeriod.AddPersonSkill(personSkill);

            return personPeriod;
        }

        public static PersonPeriod CreatePersonPeriodWithSkills(DateOnly startDate, Team team, IRuleSetBag ruleSetBag)
        {
            PersonPeriod personPeriod = CreatePersonPeriodWithSkills(startDate, team);
            personPeriod.RuleSetBag = ruleSetBag;

            return personPeriod;
        }

        /// <summary>
        /// Creates the person period.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        /// /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-29
        /// </remarks>
        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate, ITeam team)
        {
            IPersonContract personContract =
               new PersonContract(new Contract("my first contract"), new PartTimePercentage("Testing"), new ContractSchedule("Test1"));

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

        /// <summary>
        /// Creates  the person period
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="personContract"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public static IPersonPeriod CreatePersonPeriod(DateOnly startDate, IPersonContract personContract, ITeam team)
        {
            IPersonPeriod personPeriod = new PersonPeriod(startDate, personContract, team);

            IGroupingActivity groupActivity = new GroupingActivity("dummy group activity");
            IActivity activity = new Activity("dummy activity");
            activity.GroupingActivity = groupActivity;

            return personPeriod;
        }
    }
}
