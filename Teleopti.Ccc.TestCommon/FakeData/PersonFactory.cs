﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class PersonFactory
    {
        #region Simple factory methods

        /// <summary>
        /// Creates a person.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IPerson CreatePerson()
        {
            return CreatePerson("arne");
        }

        /// <summary>
        /// Creates a person.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IPerson CreatePerson(string name)
        {
            return CreatePerson(name, name);
        }

        /// <summary>
        /// Creates a person.
        /// </summary>
        /// <param name="firstName">Name of the first.</param>
        /// <param name="lastName">Name of the last.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IPerson CreatePerson(string firstName, string lastName)
        {
            Name name = new Name(firstName, lastName);
            return CreatePerson(name);
        }

        /// <summary>
        /// Creates a person.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IPerson CreatePerson(Name name)
        {
            var ret = new Person { Name = name };
            ret.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.Utc));

            return ret;
        }

        public static IPerson CreatePersonWithGuid(string firstName, string lastName)
        {
            var ret = CreatePerson(firstName, lastName);
            ret.SetId(Guid.NewGuid());
            return ret;
        }

        public static IPerson GetPerson(IPerson person, IList<IPersonPeriod> personPeriodList)
        {
            // Add rule set bags to person periods
            for (int index = 0; index < personPeriodList.Count; index++)
            {
                personPeriodList[index].RuleSetBag = new RuleSetBag();
                person.AddPersonPeriod(personPeriodList[index]);
            }

            return person;
        }

        public static IPerson CreatePersonWithPersonPeriod(DateOnly personPeriodStart, IEnumerable<ISkill> skillsInPersonPeriod)
        {
         
            return CreatePersonWithPersonPeriod(new Person(), personPeriodStart, skillsInPersonPeriod);
        }


        public static IPerson CreatePersonWithValidVirtualSchedulePeriod(IPerson person, DateOnly periodStart)
        {
            IPerson retPerson = CreatePersonWithPersonPeriod(person, periodStart, new List<ISkill>());
            ISchedulePeriod schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(periodStart);
            retPerson.AddSchedulePeriod(schedulePeriod);
            return retPerson;
        }

        public static IPerson CreatePersonWithPersonPeriod(IPerson person, DateOnly personPeriodStart, IEnumerable<ISkill> skillsInPersonPeriod)
        {
            
            IPersonPeriod pPeriod = new PersonPeriod(personPeriodStart,
                                                    PersonContractFactory.CreatePersonContract(),
                                                     new Team());
            foreach (ISkill skill in skillsInPersonPeriod)
            {
                IPersonSkill pSkill = new PersonSkill(skill, new Percent(1)) {Active = true};
            	if(skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
				{
					pPeriod.AddPersonMaxSeatSkill(pSkill);
				}
                else if (skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
                {
                    pPeriod.AddPersonNonBlendSkill(pSkill);
                }
				else
				{
					pPeriod.AddPersonSkill(pSkill);
				}
                
            }
            person.AddPersonPeriod(pPeriod);
            return person;
        }


        #endregion

        #region Complex factory methods for test packages

        /// <summary>
        /// Creates a person with basic permission info.
        /// </summary>
        /// <param name="logOnName">Name of the log on.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IPerson CreatePersonWithBasicPermissionInfo(string logOnName, string password)
        {
            ApplicationAuthenticationInfo applicationPer = new ApplicationAuthenticationInfo();
            applicationPer.ApplicationLogOnName = logOnName;
            applicationPer.Password = password;
            IPerson ret = CreatePerson("created by", "object mother");
            ret.PermissionInformation.ApplicationAuthenticationInfo = applicationPer;
            return ret;
        }

        /// <summary>
        /// Creates the person with windows permission info.
        /// </summary>
        /// <param name="logOnName">Name of the log on.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IPerson CreatePersonWithWindowsPermissionInfo(string logOnName, string domainName)
        {
            WindowsAuthenticationInfo winPer = new WindowsAuthenticationInfo();
            winPer.WindowsLogOnName = logOnName;
            winPer.DomainName = domainName;
            IPerson ret = CreatePerson("Created", "by object mother");
            ret.PermissionInformation.WindowsAuthenticationInfo = winPer;
            return ret;
        }

        /// <summary>
        /// Creates a person with application roles and functions.
        /// </summary>
        /// <returns></returns>
        public static IPerson CreatePersonWithApplicationRolesAndFunctions()
        {
            IPerson person = CreatePerson("FirstName", "LastName");
            IList<IApplicationRole> roles = ApplicationRoleFactory.CreateApplicationRolesAndFunctionsStructure();
            foreach (IApplicationRole role in roles)
            {
                person.PermissionInformation.AddApplicationRole(role);
            }
            return person;
        }

        #endregion

		public static void AddDefinitionSetToPerson(IPerson person, IMultiplicatorDefinitionSet definitionSet)
		{
			IContract ctr = new Contract("for test");
			ctr.AddMultiplicatorDefinitionSetCollection(definitionSet);
			ITeam team = new Team { Description = new Description("test team") };
			IPersonPeriod per = new PersonPeriod(new DateOnly(1900, 1, 1),
												 new PersonContract(ctr, new PartTimePercentage("f"), new ContractSchedule("f")),
												team);
			person.AddPersonPeriod(per);
		}
    }
}
