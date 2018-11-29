using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.WorkflowControl;


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
	        return CreatePerson(name, TimeZoneInfoFactory.UtcTimeZoneInfo());
        }

		/// <summary>
		/// Creates a person with an give timezone.
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="timeZoneInfo">Timezone</param>
		/// <returns></returns>
		public static IPerson CreatePerson(Name name, TimeZoneInfo timeZoneInfo)
		{
			var ret = new Person().WithName(name);
			ret.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
			return ret;
		}
		
		public static IPerson CreatePerson(TimeZoneInfo timeZoneInfo)
		{
			return CreatePerson(new Name("arne", "arne"), timeZoneInfo);
		}

		public static IPerson CreatePerson(WorkflowControlSet wfcs)
		{
			var ret = CreatePerson();
			ret.WorkflowControlSet = wfcs;
			return ret;
		}

		public static IPerson CreatePersonWithId()
		{
			return CreatePerson().WithId();
		}

		public static IPerson CreatePersonWithId(Guid personId)
		{
			var person = CreatePerson();
			person.SetId(personId);
			return person;
		}

		public static IPerson CreatePersonWithSchedulePublishedToDate(DateOnly dateOnly)
		{
			var ret = CreatePerson().WithId();
			ret.WorkflowControlSet = new WorkflowControlSet
				{
					SchedulePublishedToDate = dateOnly.Date
				};
			return ret;
		}

        public static IPerson CreatePersonWithGuid(string firstName, string lastName)
        {
            return CreatePerson(firstName, lastName).WithId();
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

		public static IPerson CreatePersonWithPersonPeriod(DateOnly personPeriodStart)
		{
			return CreatePersonWithPersonPeriod(new Person(), personPeriodStart, new ISkill[] { }, new Contract("ctr"), new PartTimePercentage("ptc"));
		}

		public static IPerson CreatePersonWithPersonPeriodTeamSite(DateOnly personPeriodStart)
		{
			var site = SiteFactory.CreateSimpleSite("Site").WithId();
			var team = new Team {Site = site}
				.WithDescription(new Description("Team 1"));
			var person = CreatePersonWithPersonPeriod(new Person(), personPeriodStart, new ISkill[] { }, team, new Contract("ctr"), new PartTimePercentage("ptc"));
			return person;
		}
		
		public static IPerson CreatePersonWithPersonPeriodFromTeam(DateOnly personPeriodStart, ITeam team)
		{
			var person = CreatePersonWithPersonPeriod(new Person(), personPeriodStart, new ISkill[] { }, team, new Contract("ctr"), new PartTimePercentage("ptc"));
			return person.WithId();
		}

		public static IPerson CreatePersonWithPersonPeriodFromTeam(Guid personId, DateOnly personPeriodStart, ITeam team)
		{
			var person = CreatePersonWithPersonPeriod(new Person(), personPeriodStart, new ISkill[] { }, team, new Contract("ctr"), new PartTimePercentage("ptc"));
			return person.WithId(personId);
		}

		public static IPerson CreatePersonWithPersonPeriod(DateOnly personPeriodStart, IEnumerable<ISkill> skillsInPersonPeriod)
	    {
				return CreatePersonWithPersonPeriod(new Person(), personPeriodStart, skillsInPersonPeriod, new Contract("ctr"), new PartTimePercentage("ptc"));
	    }

        public static IPerson CreatePersonWithValidVirtualSchedulePeriod(IPerson person, DateOnly periodStart)
        {
					return CreatePersonWithValidVirtualSchedulePeriod(person, periodStart, new Contract("ctr"), new PartTimePercentage("ptc"));
        }

				public static IPerson CreatePersonWithValidVirtualSchedulePeriod(IPerson person, DateOnly periodStart, IContract contract, IPartTimePercentage partTimePercentage)
				{
					IPerson retPerson = CreatePersonWithPersonPeriod(person, periodStart, new List<ISkill>(), contract, partTimePercentage);
					ISchedulePeriod schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(periodStart);
					retPerson.AddSchedulePeriod(schedulePeriod);
					return retPerson;
				}

		public static IPerson CreatePersonWithValidVirtualSchedulePeriodAndMustHave(IPerson person, DateOnly periodStart)
		{
			IPerson retPerson = CreatePersonWithPersonPeriod(person, periodStart, new List<ISkill>(), new Contract("ctr"), new PartTimePercentage("ptc"));
			ISchedulePeriod schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(periodStart);
			schedulePeriod.MustHavePreference = 2;
			retPerson.AddSchedulePeriod(schedulePeriod);
			return retPerson;
		}

        public static IPerson CreatePersonWithPersonPeriod(IPerson person, DateOnly personPeriodStart, IEnumerable<ISkill> skillsInPersonPeriod, IContract contract, IPartTimePercentage partTimePercentage)
        {
	        return CreatePersonWithPersonPeriod(person, personPeriodStart, skillsInPersonPeriod, new Team(), contract, partTimePercentage);
        }

		private static IPerson CreatePersonWithPersonPeriod(IPerson person, DateOnly personPeriodStart, IEnumerable<ISkill> skillsInPersonPeriod, ITeam teamInPersonPeriod, IContract contract, IPartTimePercentage partTimePercentage)
		{
			IPersonPeriod pPeriod = person.Period(personPeriodStart);
			if (pPeriod == null)
			{
				pPeriod = new PersonPeriod(personPeriodStart,
								 PersonContractFactory.CreatePersonContract(contract, partTimePercentage, new ContractSchedule("contract schedule name")),
								 teamInPersonPeriod);
				person.AddPersonPeriod(pPeriod);
			}
			foreach (ISkill skill in skillsInPersonPeriod)
			{
				IPersonSkill pSkill = new PersonSkill(skill, new Percent(1));
				if (skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
				{
					throw new NotSupportedException();
				}
				else if (skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
				{
					pPeriod.AddPersonNonBlendSkill(pSkill);
				}
				else
				{
					person.AddSkill(pSkill, pPeriod);
				}

			}
			return person;
		}


        #endregion

        #region Complex factory methods for test packages

        /// <summary>
        /// Creates a person with application roles and functions.
        /// </summary>
        /// <returns></returns>
        public static IPerson CreatePersonWithApplicationRolesAndFunctions()
        {
            return AddApplicationRolesAndFunctions(CreatePerson("FirstName", "LastName"));
        }

		/// <summary>
		/// Adds application roles and functions to an existing person.
		/// </summary>
		/// <returns></returns>
		public static IPerson AddApplicationRolesAndFunctions(IPerson person)
		{
			IList<IApplicationRole> roles = ApplicationRoleFactory.CreateApplicationRolesAndFunctionsStructure();
			foreach (IApplicationRole role in roles)
			{
				role.WithId();
				person.PermissionInformation.AddApplicationRole(role);
			}
			return person;
		}

		#endregion

		public static void AddDefinitionSetToPerson(IPerson person, IMultiplicatorDefinitionSet definitionSet)
		{
			IContract ctr = new Contract("for test");
			ctr.AddMultiplicatorDefinitionSetCollection(definitionSet);
			ITeam team = new Team().WithDescription(new Description("test team"));
			IPersonPeriod per = new PersonPeriod(new DateOnly(1900, 1, 1),
												 new PersonContract(ctr, new PartTimePercentage("f"), new ContractSchedule("f")),
												team);
			person.AddPersonPeriod(per);
		}

    }
}
