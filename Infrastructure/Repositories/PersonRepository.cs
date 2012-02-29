﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Obfuscated.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-28
    /// </remarks>
    public class PersonRepository : Repository<IPerson>, IPersonRepository
    {
        public PersonRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public PersonRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

        /// <summary>
        /// Tries to find a basic authenticated user.
        /// </summary>
        /// <param name="logOnName">The logOnName.</param>
        /// <returns></returns>
        public IPerson TryFindBasicAuthenticatedPerson(string logOnName)
        {
            using (PerformanceOutput.ForOperation("Trying to find basic auth person in db"))
            {
                IPerson foundPerson = isSuperUser(logOnName) ?
                    createSuperUserCriteria().UniqueResult<Person>() :
                    createApplicationLogonNameCriteria(logOnName).UniqueResult<Person>();

                if (foundPerson != null)
                {
                    foundPerson = LoadPermissionData(foundPerson);
                }
                return foundPerson;
            }
        }

        /// <summary>
        /// Tries to find a windows authenticated user.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="logOnName">Name of the log on.</param>
        /// <param name="foundPerson">The found user.</param>
        /// <returns></returns>
        public bool TryFindWindowsAuthenticatedPerson(string domainName, string logOnName, out IPerson foundPerson)
        {
            using (PerformanceOutput.ForOperation("Trying to find windows auth person in db"))
            {
                foundPerson = createWindowsLogonNameCriteria(domainName, logOnName).UniqueResult<Person>();

                if (foundPerson == null) return false;

                foundPerson = LoadPermissionData(foundPerson);
                return true;
            }
        }

        /// <summary>
        /// Determines whether the username and password belongs to the super user.
        /// </summary>
        /// <param name="logOnName">Name of the user.</param>
        /// <returns>
        /// 	<c>true</c> if super user; otherwise, <c>false</c>.
        /// </returns>
        private static bool isSuperUser(string logOnName)
        {
            return (logOnName == SuperUser.UserName);
        }

        private ICriteria createApplicationLogonNameCriteria(string logOnName)
        {
            return Session.CreateCriteria(typeof(Person), "person")
                .Add(Restrictions.Eq("PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName",
                                     logOnName))
                .Add(Restrictions.Disjunction()
                         .Add(Restrictions.IsNull("TerminalDate"))
                         .Add(Restrictions.Ge("TerminalDate", DateOnly.Today)));
        }

        private ICriteria createSuperUserCriteria()
        {

            //todo
            //should be case sensitive in password but not on user name
            //what if sql server instance is case insensitive - should the username still be incasesensitive?
            return Session.CreateCriteria(typeof(Person), "person")
                .Add(Restrictions.Eq("Id", new Guid(SuperUser.Id)));
        }

        /// <summary>
        /// Finds the persons that also are users.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-11-05
        /// </remarks>
        public IList<IPerson> FindPersonsThatAlsoAreUsers()
        {
            return Session.CreateCriteria(typeof(Person), "person")
                .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team.Site", FetchMode.Join)
                //BusinessUnitExplicit
                .SetFetchMode("PersonPeriodCollection.Team.Site.BusinessUnit", FetchMode.Join)
                .Add(Restrictions.Or(
                         Restrictions.Not(
                             Restrictions.Eq(
                                 "PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName",
                                 String.Empty)),
                         Restrictions.Not(
                             Restrictions.Eq("PermissionInformation.WindowsAuthenticationInfo.WindowsLogOnName",
                                             String.Empty)))).SetResultTransformer(Transformers.DistinctRootEntity).List<IPerson>();
        }

		public bool DoesWindowsUserExists(string domainName, string userName)
		{
			var res = Session.GetNamedQuery("CheckIfWindowsUserExists")
					.SetString("userName", userName)
					.SetString("domainName", domainName)
					.SetDateTime("dateNow", new DateOnly(DateTime.Now))
					.UniqueResult();

			return res != null;
		}


        private ICriteria createWindowsLogonNameCriteria(string domainName, string logOnName)
        {

            //todo
            //should be case sensitive in password but not on user name
            //what if sql server instance is case insensitive - should the username still be incasesensitive?
            return Session.CreateCriteria(typeof(Person), "person")
                .Add(Restrictions.Eq("PermissionInformation.WindowsAuthenticationInfo.WindowsLogOnName", logOnName))
                .Add(Restrictions.Disjunction()
                        .Add(Restrictions.IsNull("TerminalDate"))
                        .Add(Restrictions.Ge("TerminalDate", new DateOnly(DateTime.Now))))
                .Add(Restrictions.Eq("PermissionInformation.WindowsAuthenticationInfo.DomainName", domainName));
        }


        /// <summary>
        /// Gets a value indicating if user must be logged in to use repository.
        /// This repository returns false.
        /// </summary>
        /// <value><c>true</c> if validation should occur; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-30
        /// </remarks>
        public override bool ValidateUserLoggedOn
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Loads all person with hierarchy data sort by name.
        /// </summary>
        /// <returns></returns>
        public ICollection<IPerson> LoadAllPeopleWithHierarchyDataSortByName(DateOnly earliestTerminalDate)
        {
            try
            {
                var identity = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
                var personsubQuery = DetachedCriteria.For<PersonPeriod>("personPeriod")
                    .CreateAlias("Team", "team", JoinType.InnerJoin)
                    .CreateAlias("team.Site", "site", JoinType.InnerJoin)
                    .Add(Restrictions.Eq("site.BusinessUnit", identity.BusinessUnit))
                    .SetProjection(Projections.Property("personPeriod.Parent"));

                var personPeriodSubQuery = DetachedCriteria.For<Person>("users")
                    .CreateAlias("PersonPeriodCollection", "personPeriod")
                    .Add(Restrictions.Disjunction()
                             .Add(Restrictions.IsNull("TerminalDate"))
                             .Add(Restrictions.Ge("TerminalDate", earliestTerminalDate)
                             ))
                    .SetProjection(Projections.Property("personPeriod.Id"));

                var list = Session.CreateMultiCriteria()
                    .Add(DetachedCriteria.For<Person>("users")
                        .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                        .Add(Restrictions.IsEmpty("PersonPeriodCollection"))
                        .Add(Restrictions.Disjunction()
                                     .Add(Restrictions.IsNull("TerminalDate"))
                                     .Add(Restrictions.Ge("TerminalDate", earliestTerminalDate)
                        )))

                    .Add(DetachedCriteria.For<Person>("per")
                        .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                        .Add(Subqueries.PropertyIn("per.Id", personsubQuery))
                        .Add(Restrictions.Disjunction()
                                     .Add(Restrictions.IsNull("TerminalDate"))
                                     .Add(Restrictions.Ge("TerminalDate", earliestTerminalDate)
                         ))
                        .SetResultTransformer(Transformers.DistinctRootEntity))
                    .Add(DetachedCriteria.For<PersonPeriod>()
                        .SetFetchMode("Team", FetchMode.Join)
                        .SetFetchMode("Team.Site", FetchMode.Join)
                        .SetFetchMode("Team.Site.BusinessUnit", FetchMode.Join)
                        .CreateAlias("Team", "team", JoinType.InnerJoin)
                        .CreateAlias("team.Site", "site", JoinType.InnerJoin)
                        .Add(Restrictions.Eq("site.BusinessUnit", identity.BusinessUnit))
                        .SetResultTransformer(Transformers.DistinctRootEntity))
                    .Add(DetachedCriteria.For<PersonPeriod>("personPeriod")
                        .Add(Subqueries.PropertyIn("personPeriod.Id", personPeriodSubQuery))
                        .SetFetchMode("PersonSkillCollection", FetchMode.Join)
                        .SetResultTransformer(Transformers.DistinctRootEntity))
                    .List();

                var result = new List<IPerson>();
                result.AddRange(CollectionHelper.ToDistinctGenericCollection<IPerson>(list[0]));
                result.AddRange(CollectionHelper.ToDistinctGenericCollection<IPerson>(list[1]));

                return result.OrderBy(p => p.Name.LastName).ThenBy(p => p.Name.FirstName).ToArray();
            }
            catch (SqlException sqlException)
            {
                throw new DataSourceException(sqlException.Message, sqlException);
            }

        }

        /// <summary>
        /// Finds the people belong team.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-13
        /// </remarks>
        public ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateTimePeriod period)
        {
            ICollection<IPerson> retList = Session.CreateCriteria(typeof(Person), "per")
                      .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                      .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
                      .Add(Subqueries.Exists(findActivePeriod(team, period)))
                      .AddOrder(Order.Asc("Name.LastName"))
                      .AddOrder(Order.Asc("Name.FirstName"))
                      .SetResultTransformer(Transformers.DistinctRootEntity)
                      .List<IPerson>();

            return retList;

        }

		public IEnumerable<IPerson> FindPeopleBelongTeamPeriodInUtc(ITeam team, DateTimePeriod period)
		{
			return Session.CreateCriteria(typeof(Person), "per")
					  .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
					  .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
					  .Add(Subqueries.Exists(findActivePeriod(team, period.StartDateTime, period.EndDateTime)))
					  .AddOrder(Order.Asc("Name.LastName"))
					  .AddOrder(Order.Asc("Name.FirstName"))
					  .SetResultTransformer(Transformers.DistinctRootEntity)
					  .List<IPerson>();
		}

		public ICollection<IPerson> FindPeopleBelongTeamWithSchedulePeriod(ITeam team, DateTimePeriod period)
		{
			ICollection<IPerson> tempList = Session.CreateCriteria(typeof(Person), "per")
					  .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
					  .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
					  .Add(Subqueries.Exists(findActivePeriod(team, period)))
					  .AddOrder(Order.Asc("Name.LastName"))
					  .AddOrder(Order.Asc("Name.FirstName"))
					  .SetResultTransformer(Transformers.DistinctRootEntity)
					  .List<IPerson>();


			//to get all we need (scheduleperiod for example)
			return FindPeople(tempList);

		}

       public ICollection<IPerson> FindPeopleByEmploymentNumber(string employmentNumber)
        {
            ICollection<IPerson> retList = Session.CreateCriteria(typeof(Person), "per")
                       .Add(Restrictions.Eq("EmploymentNumber", employmentNumber))
                      .SetResultTransformer(Transformers.DistinctRootEntity)
                      .List<IPerson>();
            return retList;
        }

        public int NumberOfActiveAgents()
        {
            const string buFilterName = "businessUnitFilter";
            DateTime now = DateTime.UtcNow;

            //dirty duplicated code here - fix and move
            var identity = Thread.CurrentPrincipal.Identity as TeleoptiIdentity;
            Guid buId = identity != null && identity.BusinessUnit != null
                            ? identity.BusinessUnit.Id.GetValueOrDefault()
                            : Guid.Empty;
            Session.DisableFilter(buFilterName);
            int totalInAllBusinessUnits = (int)Session.GetNamedQuery("ActiveAgents")
                                    .SetDateTime("currentDate", now)
                                    .UniqueResult<long>();
            Session.EnableFilter(buFilterName).SetParameter("businessUnitParameter", buId);
            return totalInAllBusinessUnits;
        }

        /// <summary>
        /// Finds all persons with teams name sorting.
        /// </summary>
        /// <returns></returns>
        public ICollection<IPerson> FindAllSortByName()
        {
            ICollection<IPerson> retList = Session.CreateCriteria(typeof(Person))
                        .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                        .AddOrder(Order.Asc("Name.LastName"))
                        .AddOrder(Order.Asc("Name.FirstName"))
                        .SetResultTransformer(Transformers.DistinctRootEntity)
                        .List<IPerson>();
            return retList;
        }


    	/// <summary>
        /// Loads the permission data to the person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public IPerson LoadPermissionData(IPerson person)
        {
            var personPeriods = DetachedCriteria.For<Person>()
                .Add(Restrictions.Eq("Id", person.Id))
                .SetFetchMode("PersonPeriodCollection", FetchMode.Join);

            var teamSubquery = DetachedCriteria.For<PersonPeriod>("pp")
                .Add(Restrictions.Eq("pp.Parent", person))
                .CreateAlias("Team", "team")
                .SetProjection(Projections.Property("team.Id"));

            var teams = DetachedCriteria.For<Team>()
                .Add(Subqueries.PropertyIn("Id", teamSubquery))
                .SetFetchMode("Site", FetchMode.Join)
                .SetFetchMode("Site.TeamCollection", FetchMode.Join);

            var bus = DetachedCriteria.For<Team>()
                .Add(Subqueries.PropertyIn("Id", teamSubquery))
                .SetFetchMode("BusinessUnitExplicit", FetchMode.Join);

            var roles = DetachedCriteria.For<Person>()
                .Add(Restrictions.Eq("Id", person.Id))
                .SetFetchMode("PermissionInformation.personInApplicationRole", FetchMode.Join);
            
            var functions = DetachedCriteria.For<ApplicationRole>()
                .SetFetchMode("ApplicationFunctionCollection", FetchMode.Join)
                .SetFetchMode("BusinessUnit", FetchMode.Join);
            
            Session.DisableFilter("businessUnitFilter");
            using (UnitOfWork.DisableFilter(QueryFilter.Deleted))
            {
                var result =
                    Session.CreateMultiCriteria().Add(personPeriods).Add(teams).Add(bus).Add(roles).Add(functions).List();
                var foundPerson = CollectionHelper.ToDistinctGenericCollection<Person>(result[0]).First();

                return foundPerson;
            }
        }

        /// <summary>
        /// Loads the permission data without reassociate.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-30
        /// </remarks>
        public IPerson LoadPermissionDataWithoutReassociate(IPerson person)
        {
            LazyLoadingManager.Initialize(person.PermissionInformation);
            LazyLoadingManager.Initialize(person.PermissionInformation.ApplicationRoleCollection);

            foreach (IApplicationRole role in person.PermissionInformation.ApplicationRoleCollection)
            {
                LazyLoadingManager.Initialize(role.ApplicationFunctionCollection);
            }

            return person;
        }

		public ICollection<IPerson> FindPeopleTeamSiteSchedulePeriodWorkflowControlSet(DateTimePeriod period)
		{
			var multiCrit = Session.CreateMultiCriteria()
				.Add(personPeriodsOnlyTeamAndSite(period))
				.Add(personPeriodSkills(period))
				.Add(personSchedule(period))
				.Add(personWorkflowControlSet());

			var res = multiCrit.List();
			var persons = CollectionHelper.ToDistinctGenericCollection<IPerson>(res[0]);

			return persons;
		}

        public ICollection<IPerson> FindPeopleInOrganizationLight(DateTimePeriod period)
        {
            IMultiCriteria multiCrit = Session.CreateMultiCriteria()
                .Add(personPeriodsOnlyTeamAndSite(period));

            IList res = multiCrit.List();
            ICollection<IPerson> persons = CollectionHelper.ToDistinctGenericCollection<IPerson>(res[0]);

            return persons;
        }

        public ICollection<IPerson> FindPeople(IEnumerable<Guid> peopleId)
        {
            var result = new List<IPerson>();
            foreach (var peopleBatch in peopleId.Batch(200))
            {
                var currentBatchIds = peopleBatch.ToArray();

                DetachedCriteria person = DetachedCriteria.For<Person>("person")
                    .Add(Restrictions.InG("Id", currentBatchIds))
                    //.SetFetchMode("PermissionInformation", FetchMode.Join)
                    .SetFetchMode("PermissionInformation.personInApplicationRole", FetchMode.Join);

                DetachedCriteria personPeriod = DetachedCriteria.For<Person>("personPeriod")
                    .Add(Restrictions.InG("Id", currentBatchIds))
					.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.Team.Site", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.Team.Site.BusinessUnit", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.PersonSkillCollection", FetchMode.Join);

            	DetachedCriteria schedulePeriod = DetachedCriteria.For<Person>("schedulePeriod")
            		.Add(Restrictions.InG("Id", currentBatchIds))
					.SetFetchMode("PersonSchedulePeriodCollection", FetchMode.Join)
            		.SetFetchMode("PersonSchedulePeriodCollection.shiftCategoryLimitation", FetchMode.Join);
					
                IList queryResult = Session.CreateMultiCriteria()
                    .Add(person)
                    .Add(personPeriod)
                    .Add(schedulePeriod)
                    .List();

                var foundPeople = CollectionHelper.ToDistinctGenericCollection<IPerson>(queryResult[0]);

                result.AddRange(foundPeople);
            }
            return result;
        }

        public ICollection<IPerson> FindPeople(IEnumerable<IPerson> people)
        {
            var peopleId = people.Select(p => p.Id.GetValueOrDefault());
            return FindPeople(peopleId);
        }

    	public ICollection<IPerson> FindPeopleInOrganization(DateTimePeriod period, bool includeRuleSetData)
        {
            IMultiCriteria multiCrit = Session.CreateMultiCriteria()
                                    .Add(personPeriodTeamAndSites(period))
                                    .Add(personPeriodSkills(period))
                                    .Add(personSchedule(period))
                                    .Add(personPeriodLogOns(period))
                                    .Add(personWorkflowControlSet());
            personPeriodContract(period).ForEach(crit => multiCrit.Add(crit));

            //gör detta smartare! 
            if (includeRuleSetData)
            {
                using (UnitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    //Bryt ut till rätt klass (=annat rep)!
                    Session.CreateCriteria(typeof(RuleSetBag))
                        .SetFetchMode("RuleSetCollection", FetchMode.Join)
                        .List();
                    new WorkShiftRuleSetRepository(UnitOfWork).FindAllWithLimitersAndExtenders();
                }
            }

            IList res = multiCrit.List();
            ICollection<IPerson> persons = CollectionHelper.ToDistinctGenericCollection<IPerson>(res[0]);

            //ta bort detta - gammalt blajj. ska inte behövas om frågorna gör rätt. Gör om, gör rätt!
            //Ola: När man kör från PeopleLoader i Schedulern behövs inte detta nu. Vågar dock inte ta bort det för denna används från många ställen
            foreach (IPerson person in persons)
            {
                foreach (IPersonPeriod personPeriod in person.PersonPeriodCollection)
                {
                    foreach (PersonSkill pSkill in personPeriod.PersonSkillCollection)
                    {
                        if (pSkill.Skill.Activity.Name == "xyyyxxxyyyyx")
                            throw new InvalidDataException("lazy load elände");
                    }
                }
            }


            return persons;
        }

        private static DetachedCriteria personWorkflowControlSet()
        {
            return DetachedCriteria.For<Person>("per").SetFetchMode("WorkflowControlSet", FetchMode.Join);
        }


        private static DetachedCriteria personSchedule(DateTimePeriod period)
        {
            return DetachedCriteria.For<Person>("per")
                .Add(Restrictions.Or(
                         Restrictions.IsNull("TerminalDate"),
						 Restrictions.Ge("TerminalDate", new DateOnly(period.LocalStartDateTime))
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("PersonSchedulePeriodCollection", FetchMode.Join)
                .SetFetchMode("PersonSchedulePeriodCollection.shiftCategoryLimitation", FetchMode.Join);
        }

        private static DetachedCriteria personPeriodTeamAndSites(DateTimePeriod period)
        {
            return DetachedCriteria.For<Person>("per")
                .Add(Restrictions.Or(
                         Restrictions.IsNull("TerminalDate"),
                         Restrictions.Ge("TerminalDate", new DateOnly(period.LocalStartDateTime))
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team.Site", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team.Site.BusinessUnit", FetchMode.Join)
                .AddOrder(Order.Asc("Name.LastName"))
                .AddOrder(Order.Asc("Name.FirstName"));
        }

        private static IEnumerable<DetachedCriteria> personPeriodContract(DateTimePeriod period)
        {
            var ret = new DetachedCriteria[2];
            ret[0] = DetachedCriteria.For<Person>("per")
                        .Add(Restrictions.Or(
                                 Restrictions.IsNull("TerminalDate"),
                                 Restrictions.Ge("TerminalDate", new DateOnly(period.LocalStartDateTime))
                                 ))
                        .Add(Subqueries.Exists(findPeriodMatch(period)))
                        .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract.ContractSchedule", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract.ContractSchedule.ContractScheduleWeeks", FetchMode.Join);
            ret[1] = DetachedCriteria.For<Person>("per")
                        .Add(Restrictions.Or(
                                 Restrictions.IsNull("TerminalDate"),
                                 Restrictions.Ge("TerminalDate", new DateOnly(period.LocalStartDateTime))
                                 ))
                        .Add(Subqueries.Exists(findPeriodMatch(period)))
                        .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract.PartTimePercentage", FetchMode.Join);
            return ret;
        }

        private static DetachedCriteria personPeriodSkills(DateTimePeriod period)
        {
            return DetachedCriteria.For<PersonPeriod>("period")
                .CreateAlias("Parent", "per", JoinType.InnerJoin)
                .Add(Restrictions.Or(
                         Restrictions.IsNull("per.TerminalDate"),
                         Restrictions.Ge("per.TerminalDate", new DateOnly(period.LocalStartDateTime))
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("PersonSkillCollection", FetchMode.Join);
        }

        private static DetachedCriteria personPeriodLogOns(DateTimePeriod period)
        {
            return DetachedCriteria.For<PersonPeriod>("period")
                .CreateAlias("Parent", "per", JoinType.InnerJoin)
                .Add(Restrictions.Or(
                         Restrictions.IsNull("per.TerminalDate"),
                         Restrictions.Ge("per.TerminalDate", new DateOnly(period.LocalStartDateTime))
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("ExternalLogOnCollection", FetchMode.Join);
        }

        private static DetachedCriteria personPeriodsOnlyTeamAndSite(DateTimePeriod period)
        {
            return DetachedCriteria.For<Person>("per")
                .Add(Restrictions.Or(
                         Restrictions.IsNull("TerminalDate"),
                         Restrictions.Ge("TerminalDate", new DateOnly(period.LocalStartDateTime))
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team.Site", FetchMode.Join)
				.SetFetchMode("PersonPeriodCollection.Team.Site.BusinessUnit", FetchMode.Join);
        }

        /// <summary>
        /// Finds the period match
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        /// /// <remarks>
        /// Created by: rogerkr/cs
        /// Created date: 2008-13-05
        /// </remarks>
        private static DetachedCriteria findPeriodMatch(DateTimePeriod period)
        {
            var identity = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            DetachedCriteria detachedCriteria = DetachedCriteria.For(typeof(PersonPeriod), "first")
                .CreateAlias("Team", "team", JoinType.InnerJoin)
                .CreateAlias("team.Site", "site", JoinType.InnerJoin)
                .SetProjection(Projections.Id())
                .Add(Restrictions.Le("StartDate", new DateOnly(period.LocalEndDateTime)))
                .Add(Restrictions.EqProperty("first.Parent", "per.Id"))
                .Add(Restrictions.Eq("site.BusinessUnit", identity.BusinessUnit))
                .Add(Subqueries.NotExists(DetachedCriteria.For<PersonPeriod>()
                                               .SetProjection(Projections.Id())
                                               .Add(Restrictions.EqProperty("Parent", "first.Parent"))
                                               .Add(Restrictions.GtProperty("StartDate", "first.StartDate"))
                                               .Add(Restrictions.Le("StartDate", new DateOnly(period.LocalStartDateTime)))));
            return detachedCriteria;
        }

        public IEnumerable<IPair<Guid>> PeopleSkillMatrix(IScenario scenario, DateTimePeriod period)
        {
            var identity = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            return Session.GetNamedQuery("AgentSkillMatrix")
                    .SetEntity("bu", identity.BusinessUnit)
                    .SetEntity("scenario", scenario)
                    .SetProperties(period)
                    .List<IPair<Guid>>();
        }

        public IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period)
        {
            var identity = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            return Session.GetNamedQuery("AgentSiteMatrix")
                    .SetEntity("bu", identity.BusinessUnit)
                    .SetProperties(period)
                    .List<Guid>();
        }

        /// <summary>
        /// Finds the current period match.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-13
        /// </remarks>
        private static DetachedCriteria findActivePeriod(ITeam team, DateTimePeriod period)
        {
			// ouch, state holder deeply hidden dependency!
        	return findActivePeriod(team, period.LocalStartDateTime, period.LocalEndDateTime);
        }

		private static DetachedCriteria findActivePeriod(ITeam team, DateTime startDate, DateTime endDate)
		{
			return  DetachedCriteria.For(typeof(PersonPeriod), "first")
				.SetProjection(Projections.Id())
				.Add(Restrictions.Le("StartDate", new DateOnly(endDate)))
				.Add(Restrictions.EqProperty("first.Parent", "per.Id"))
				.Add(Restrictions.Eq("Team", team))
				.Add(Subqueries.NotExists(DetachedCriteria.For<PersonPeriod>()
											  .SetProjection(Projections.Id())
											  .Add(Restrictions.EqProperty("Parent", "first.Parent"))
											  .Add(Restrictions.Le("StartDate", new DateOnly(endDate)))
											  .Add(Restrictions.GtProperty("StartDate", "first.StartDate"))
											  .Add(Restrictions.Le("StartDate", new DateOnly(startDate)))));
		}

		

        /// <summary>
        /// Checks for the duplicated application logons and windows logons
        /// </summary>
        /// <param name="persons">The person collection.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-29
        /// Updated by: MadhurangaP
        /// </remarks>
        public IList<IPerson> FindPersonsWithGivenUserCredentials(IList<IPerson> persons)
        {
            var result = new List<IPerson>();
            foreach (var personCollection in persons.Batch(200))
            {
                IList<string[]> personInfoList = (from p in personCollection
                                                  select new[]
                                                             {
                                                                 p.PermissionInformation.WindowsAuthenticationInfo.
                                                                     WindowsLogOnName,
                                                                 p.PermissionInformation.ApplicationAuthenticationInfo.
                                                                     ApplicationLogOnName,
                                                                 p.PermissionInformation.WindowsAuthenticationInfo.
                                                                     DomainName,
                                                                 p.Id.ToString()
                                                             }).ToList();

                string[] windowsLogOns =
                    (from p in personInfoList where !String.IsNullOrEmpty(p[0]) select p[0]).ToArray();
                string[] applicationLogOns =
                    (from p in personInfoList where !String.IsNullOrEmpty(p[1]) select p[1]).ToArray();
                string[] domains = (from p in personInfoList where !String.IsNullOrEmpty(p[2]) select p[2]).ToArray();
                Guid[] winlogonNullIds =
                    (from p in personInfoList
                     where String.IsNullOrEmpty(p[0]) && String.IsNullOrEmpty(p[2])
                     select new Guid(p[3])).ToArray();
                Guid[] ids = (from p in personInfoList
                              where
                                  !String.IsNullOrEmpty(p[3]) && !String.IsNullOrEmpty(p[0]) &&
                                  !String.IsNullOrEmpty(p[2])
                              select new Guid(p[3])).ToArray();

                int idsBeforeLength = ids.Length;
                int idsAfterLength = idsBeforeLength + winlogonNullIds.Length;

                Array.Resize(ref ids, idsAfterLength);
                Array.Copy(winlogonNullIds, 0, ids, (idsBeforeLength > 0) ? (idsBeforeLength - 1) : 0,
                           winlogonNullIds.Length);

               result.AddRange(Session.CreateCriteria(typeof (IPerson)).Add
                    (
                        Restrictions.And
                            (
                                Restrictions.Or
                                    (
                                        Restrictions.In(
                                            "PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName",
                                            applicationLogOns),

                                        Restrictions.And
                                            (
                                                Restrictions.In(
                                                    "PermissionInformation.WindowsAuthenticationInfo.WindowsLogOnName",
                                                    windowsLogOns),
                                                Restrictions.In(
                                                    "PermissionInformation.WindowsAuthenticationInfo.DomainName",
                                                    domains)
                                            )
                                    ),
                                Restrictions.Not(Restrictions.In("Id", ids))
                            )
                    ).List<IPerson>());
            }
            return result;
        }
    }
}

