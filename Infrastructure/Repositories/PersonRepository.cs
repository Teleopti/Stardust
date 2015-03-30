﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
	public class PersonRepository : Repository<IPerson>, IPersonRepository, IWriteSideRepository<IPerson>, IProxyForId<IPerson>
    {
        public PersonRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public PersonRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

				public PersonRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
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
                IPerson foundPerson = createApplicationLogonNameCriteria(logOnName).UniqueResult<Person>();
                if (foundPerson != null)
                {
                    foundPerson = loadPermissionData(foundPerson);
                }
                return foundPerson;
            }
        }

	    /// <summary>
	    /// Tries to find a windows authenticated user.
	    /// </summary>
	    /// <param name="identity"></param>
	    /// <param name="foundPerson">The found user.</param>
	    /// <returns></returns>
	    public bool TryFindIdentityAuthenticatedPerson(string identity, out IPerson foundPerson)
        {
            using (PerformanceOutput.ForOperation("Trying to find windows auth person in db"))
            {
                foundPerson = createIdentityLogonNameCriteria(identity).UniqueResult<Person>();

                if (foundPerson == null) return false;

                foundPerson = loadPermissionData(foundPerson);
                return true;
            }
        }

        private ICriteria createApplicationLogonNameCriteria(string logOnName)
        {
            return Session.CreateCriteria(typeof(Person), "person")
                .Add(Restrictions.Eq("ApplicationAuthenticationInfo.ApplicationLogOnName",
                                     logOnName))
                .Add(Restrictions.Disjunction()
                         .Add(Restrictions.IsNull("TerminalDate"))
                         .Add(Restrictions.Ge("TerminalDate", DateOnly.Today)));
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
                                 "ApplicationAuthenticationInfo.ApplicationLogOnName",
                                 String.Empty)),
                         Restrictions.Not(
                             Restrictions.Eq("AuthenticationInfo.Identity",
                                             String.Empty)))).SetResultTransformer(Transformers.DistinctRootEntity).List<IPerson>();
        }


    	private ICriteria createIdentityLogonNameCriteria(string identity)
        {
            return Session.CreateCriteria(typeof(Person), "person")
                .Add(Restrictions.Eq("AuthenticationInfo.Identity", identity))
                .Add(Restrictions.Disjunction()
                        .Add(Restrictions.IsNull("TerminalDate"))
                        .Add(Restrictions.Ge("TerminalDate", DateOnly.Today)));
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
                var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
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
        public ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateOnlyPeriod period)
        {
        	ICollection<IPerson> retList = Session.CreateCriteria(typeof (Person), "per")
        		.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
        		.SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
        		.Add(Restrictions.Or(
        			Restrictions.IsNull("TerminalDate"),
        			Restrictions.Ge("TerminalDate", period.StartDate)
        		     	))
        		.Add(Subqueries.Exists(findActivePeriod(team, period)))
        		.AddOrder(Order.Asc("Name.LastName"))
        		.AddOrder(Order.Asc("Name.FirstName"))
        		.SetResultTransformer(Transformers.DistinctRootEntity)
        		.List<IPerson>();

            return retList;

        }

		public ICollection<IPerson> FindPeopleBelongTeamWithSchedulePeriod(ITeam team, DateOnlyPeriod period)
		{
			ICollection<IPerson> tempList = Session.CreateCriteria(typeof(Person), "per")
					  .SetFetchMode("OptionalColumnValueCollection",FetchMode.Join)
					  .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
					  .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
					  .Add(Restrictions.Or(
						 Restrictions.IsNull("TerminalDate"),
						 Restrictions.Ge("TerminalDate", period.StartDate)
						 ))
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
	        DateTime now = DateTime.UtcNow;
	        
			using (UnitOfWork.DisableFilter(QueryFilter.BusinessUnit))
			{
				var totalInAllBusinessUnits = (int)Session.GetNamedQuery("ActiveAgents")
									.SetDateTime("currentDate", now)
									.UniqueResult<long>();
				return totalInAllBusinessUnits;
			}
        }

				public ICollection<IPerson> FindAllSortByName()
				{
					try
					{
						var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
						var personsubQuery = DetachedCriteria.For<PersonPeriod>("personPeriod")
							.CreateAlias("Team", "team", JoinType.InnerJoin)
							.CreateAlias("team.Site", "site", JoinType.InnerJoin)
							.Add(Restrictions.Eq("site.BusinessUnit", identity.BusinessUnit))
							.SetProjection(Projections.Property("personPeriod.Parent"));

						var criterias = Session.CreateMultiCriteria()
																	 .Add(DetachedCriteria.For<Person>("users")
																												.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
																												.Add(Restrictions.IsEmpty("PersonPeriodCollection"))
							)
																	 .Add(DetachedCriteria.For<Person>("per")
																												.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
																												.Add(Subqueries.PropertyIn("per.Id", personsubQuery))
																												.SetResultTransformer(Transformers.DistinctRootEntity));

						var list = criterias.List();

						var result = new List<IPerson>();
						result.AddRange(CollectionHelper.ToDistinctGenericCollection<IPerson>(list[0]));
						result.AddRange(CollectionHelper.ToDistinctGenericCollection<IPerson>(list[1]));


						return new HashSet<IPerson>(result.OrderBy(p => p.Name.LastName).ThenBy(p => p.Name.FirstName));
					}
					catch (SqlException sqlException)
					{
						throw new DataSourceException(sqlException.Message, sqlException);
					}
				}

			public ICollection<IPerson> FindAllWithRolesSortByName()
			{
				try
				{
					var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
					var personsubQuery = DetachedCriteria.For<PersonPeriod>("personPeriod")
						.CreateAlias("Team", "team", JoinType.InnerJoin)
						.CreateAlias("team.Site", "site", JoinType.InnerJoin)
						.Add(Restrictions.Eq("site.BusinessUnit", identity.BusinessUnit))
						.SetProjection(Projections.Property("personPeriod.Parent"));

					var criterias = Session.CreateMultiCriteria()
													.Add(DetachedCriteria.For<Person>("users")
																				.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
																				.Add(Restrictions.IsEmpty("PersonPeriodCollection"))
						)
													.Add(DetachedCriteria.For<Person>("per")
																				.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
																				.Add(Subqueries.PropertyIn("per.Id", personsubQuery))
																				.SetResultTransformer(Transformers.DistinctRootEntity));

					criterias.Add(DetachedCriteria.For<Person>()
						.SetFetchMode("PermissionInformation", FetchMode.Join)
						.SetFetchMode("PermissionInformation.personInApplicationRole", FetchMode.Join));
						//.CreateAlias("PermissionInformation.personInApplicationRole", "ApplicationRoleCollection", JoinType.InnerJoin));

					var list = criterias.List();

					var result = new List<IPerson>();
					result.AddRange(CollectionHelper.ToDistinctGenericCollection<IPerson>(list[0]));
					result.AddRange(CollectionHelper.ToDistinctGenericCollection<IPerson>(list[1]));

					result.AddRange(CollectionHelper.ToDistinctGenericCollection<IPerson>(list[2]));


					return new HashSet<IPerson>(result.OrderBy(p => p.Name.LastName).ThenBy(p => p.Name.FirstName));
				}
				catch (SqlException sqlException)
				{
					throw new DataSourceException(sqlException.Message, sqlException);
				}
			}

    	/// <summary>
        /// Loads the permission data to the person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private Person loadPermissionData(IPerson person)
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

			var rolesSubquery = DetachedCriteria.For<Person>()
				.Add(Restrictions.Eq("Id", person.Id))
				.CreateAlias("PermissionInformation.personInApplicationRole","role")
				.SetProjection(Projections.Property("role.Id"));
            
            var functions = DetachedCriteria.For<ApplicationRole>()
				.Add(Subqueries.PropertyIn("Id",rolesSubquery))
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

		public ICollection<IPerson> FindPeopleTeamSiteSchedulePeriodWorkflowControlSet(DateOnlyPeriod period)
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

        public ICollection<IPerson> FindPeopleInOrganizationLight(DateOnlyPeriod period)
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
            foreach (var peopleBatch in peopleId.Batch(400))
            {
                var currentBatchIds = peopleBatch.ToArray();

                DetachedCriteria person = DetachedCriteria.For<Person>("person")
                    .Add(Restrictions.InG("Id", currentBatchIds))
                   .SetFetchMode("OptionalColumnValueCollection",FetchMode.Join);

				DetachedCriteria roles = DetachedCriteria.For<Person>("roles")
					.Add(Restrictions.InG("Id", currentBatchIds))
					.SetFetchMode("PermissionInformation.personInApplicationRole", FetchMode.Join);

                DetachedCriteria personPeriod = DetachedCriteria.For<Person>("personPeriod")
                    .Add(Restrictions.InG("Id", currentBatchIds))
					.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.Team.Site", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.Team.Site.BusinessUnit", FetchMode.Join)
                    .SetFetchMode("PersonPeriodCollection.PersonSkillCollection", FetchMode.Join)
					.SetFetchMode("PersonPeriodCollection.ExternalLogOnCollection", FetchMode.Join);

            	DetachedCriteria schedulePeriod = DetachedCriteria.For<Person>("schedulePeriod")
            		.Add(Restrictions.InG("Id", currentBatchIds))
					.SetFetchMode("PersonSchedulePeriodCollection", FetchMode.Join)
            		.SetFetchMode("PersonSchedulePeriodCollection.shiftCategoryLimitation", FetchMode.Join);
					
                IList queryResult = Session.CreateMultiCriteria()
                    .Add(person)
					.Add(roles)
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "elände"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public ICollection<IPerson> FindPeopleInOrganization(DateOnlyPeriod period, bool includeRuleSetData)
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
					foreach (var pSkill in personPeriod.PersonSkillCollection)
					{
                        if (pSkill.Skill.SkillType.Description.Name == "xyyyxxxyyyyx")
                            throw new InvalidDataException("lazy load elände");
                        if (pSkill.Skill.Activity.Name == "xyyyxxxyyyyx")
							throw new InvalidDataException("lazy load elände");

						if (!LazyLoadingManager.IsInitialized(pSkill.Skill.SkillType))
							LazyLoadingManager.Initialize(pSkill.Skill.SkillType);
					}
				}
			}


    		return persons;
        }

        private static DetachedCriteria personWorkflowControlSet()
        {
            return DetachedCriteria.For<Person>("per").SetFetchMode("WorkflowControlSet", FetchMode.Join);
        }


        private static DetachedCriteria personSchedule(DateOnlyPeriod period)
        {
            return DetachedCriteria.For<Person>("per")
                .Add(Restrictions.Or(
                         Restrictions.IsNull("TerminalDate"),
						 Restrictions.Ge("TerminalDate", period.StartDate)
                         ))
                //.Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("PersonSchedulePeriodCollection", FetchMode.Join)
                .SetFetchMode("PersonSchedulePeriodCollection.shiftCategoryLimitation", FetchMode.Join);
        }

        private static DetachedCriteria personPeriodTeamAndSites(DateOnlyPeriod period)
        {
	        return DetachedCriteria.For<Person>("per")
									.Add(Restrictions.Or(
										Restrictions.IsNull("TerminalDate"),
										Restrictions.Ge("TerminalDate", period.StartDate)
											))
									.Add(Subqueries.Exists(findPeriodMatch(period)))
									.SetFetchMode("OptionalColumnValueCollection", FetchMode.Join)
									.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
									.SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
									.SetFetchMode("PersonPeriodCollection.Team.Site", FetchMode.Join)
									.SetFetchMode("PersonPeriodCollection.Team.Site.BusinessUnit", FetchMode.Join);
	        // don't order here because it will order in tempdb and take too much resources
	        //.AddOrder(Order.Asc("Name.LastName"))
	        //.AddOrder(Order.Asc("Name.FirstName"));
        }

        private static IEnumerable<DetachedCriteria> personPeriodContract(DateOnlyPeriod period)
        {
            var ret = new DetachedCriteria[2];
            ret[0] = DetachedCriteria.For<Person>("per")
                        .Add(Restrictions.Or(
                                 Restrictions.IsNull("TerminalDate"),
                                 Restrictions.Ge("TerminalDate", period.StartDate)
                                 ))
                        .Add(Subqueries.Exists(findPeriodMatch(period)))
                        .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract.ContractSchedule", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract.ContractSchedule.ContractScheduleWeeks", FetchMode.Join);
            ret[1] = DetachedCriteria.For<Person>("per")
                        .Add(Restrictions.Or(
                                 Restrictions.IsNull("TerminalDate"),
                                 Restrictions.Ge("TerminalDate", period.StartDate)
                                 ))
                        .Add(Subqueries.Exists(findPeriodMatch(period)))
                        .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract.PartTimePercentage", FetchMode.Join)
                        .SetFetchMode("PersonPeriodCollection.PersonContract.Contract", FetchMode.Join);
            return ret;
        }

        private static DetachedCriteria personPeriodSkills(DateOnlyPeriod period)
        {
            return DetachedCriteria.For<PersonPeriod>("period")
                .CreateAlias("Parent", "per", JoinType.InnerJoin)
                .Add(Restrictions.Or(
                         Restrictions.IsNull("per.TerminalDate"),
                         Restrictions.Ge("per.TerminalDate", period.StartDate)
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("PersonSkillCollection", FetchMode.Join);
        }

        private static DetachedCriteria personPeriodLogOns(DateOnlyPeriod period)
        {
            return DetachedCriteria.For<PersonPeriod>("period")
                .CreateAlias("Parent", "per", JoinType.InnerJoin)
                .Add(Restrictions.Or(
                         Restrictions.IsNull("per.TerminalDate"),
                         Restrictions.Ge("per.TerminalDate", period.StartDate)
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("ExternalLogOnCollection", FetchMode.Join);
        }

        private static DetachedCriteria personPeriodsOnlyTeamAndSite(DateOnlyPeriod period)
        {
            return DetachedCriteria.For<Person>("per")
                .Add(Restrictions.Or(
                         Restrictions.IsNull("TerminalDate"),
                         Restrictions.Ge("TerminalDate", period.StartDate)
                         ))
                .Add(Subqueries.Exists(findPeriodMatch(period)))
                .SetFetchMode("PersonPeriodCollection", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team", FetchMode.Join)
                .SetFetchMode("PersonPeriodCollection.Team.Site", FetchMode.Join)
				.SetFetchMode("PersonPeriodCollection.Team.Site.BusinessUnit", FetchMode.Join);
        }

        private static DetachedCriteria findPeriodMatch(DateOnlyPeriod period)
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
            DetachedCriteria detachedCriteria = DetachedCriteria.For(typeof(PersonPeriod), "first")
                .CreateAlias("Team", "team", JoinType.InnerJoin)
                .CreateAlias("team.Site", "site", JoinType.InnerJoin)
                .SetProjection(Projections.Id())
                .Add(Restrictions.Le("StartDate", period.EndDate))
                .Add(Restrictions.EqProperty("first.Parent", "per.Id"))
                .Add(Restrictions.Eq("site.BusinessUnit", identity.BusinessUnit))
                .Add(Subqueries.NotExists(DetachedCriteria.For<PersonPeriod>()
                                               .SetProjection(Projections.Id())
                                               .Add(Restrictions.EqProperty("Parent", "first.Parent"))
                                               .Add(Restrictions.GtProperty("StartDate", "first.StartDate"))
                                               .Add(Restrictions.Le("StartDate", period.StartDate))));
            return detachedCriteria;
        }

        public IEnumerable<Tuple<Guid, Guid>> PeopleSkillMatrix(IScenario scenario, DateTimePeriod period)
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
            return Session.GetNamedQuery("AgentSkillMatrix")
                    .SetEntity("bu", identity.BusinessUnit)
                    .SetEntity("scenario", scenario)
                    .SetProperties(period)
										.List<Tuple<Guid, Guid>>();
        }

        public IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period)
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
            return Session.GetNamedQuery("AgentSiteMatrix")
                    .SetEntity("bu", identity.BusinessUnit)
                    .SetProperties(period)
                    .List<Guid>();
        }

		private static DetachedCriteria findActivePeriod(ITeam team, DateOnlyPeriod dateOnlyPeriod)
		{
			return  DetachedCriteria.For(typeof(PersonPeriod), "first")
				.SetProjection(Projections.Id())
				.Add(Restrictions.Le("StartDate", dateOnlyPeriod.EndDate))
				.Add(Restrictions.EqProperty("first.Parent", "per.Id"))
				.Add(Restrictions.Eq("Team", team))
				.Add(Subqueries.NotExists(DetachedCriteria.For<PersonPeriod>()
											  .SetProjection(Projections.Id())
											  .Add(Restrictions.EqProperty("Parent", "first.Parent"))
											  .Add(Restrictions.Le("StartDate", dateOnlyPeriod.EndDate))
											  .Add(Restrictions.GtProperty("StartDate", "first.StartDate"))
											  .Add(Restrictions.Le("StartDate", dateOnlyPeriod.StartDate))));
		}

        public IList<IPerson> FindPersonsWithGivenUserCredentials(IList<IPerson> persons)
        {
            var result = new List<IPerson>();
            foreach (var personCollection in persons.Batch(200))
            {
                var personInfoList = personCollection.Select(p => new {
                                                                 Identity = p.AuthenticationInfo == null ? "" : p.AuthenticationInfo.Identity,
                                                                ApplicationAuthentication = p.ApplicationAuthenticationInfo == null ? "" : p.ApplicationAuthenticationInfo.ApplicationLogOnName, p.Id
                                                             }).ToList();

				string[] windowsLogOns = personInfoList.Where(p => !String.IsNullOrEmpty(p.Identity)).Select(p => p.Identity).ToArray();
				string[] applicationLogOns = personInfoList.Where(p => !String.IsNullOrEmpty(p.ApplicationAuthentication)).Select(p => p.ApplicationAuthentication).ToArray();
                Guid[] winlogonNullIds =
                    (from p in personInfoList
                     where String.IsNullOrEmpty(p.Identity) 
                     select p.Id.GetValueOrDefault()).ToArray();
                Guid[] ids = (from p in personInfoList
                              where
                                  p.Id.HasValue && 
                                  !String.IsNullOrEmpty(p.Identity)
                              select p.Id.GetValueOrDefault()).ToArray();

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
                                            "ApplicationAuthenticationInfo.ApplicationLogOnName",
                                            applicationLogOns),
                                        Restrictions.In(
                                                    "AuthenticationInfo.Identity",
                                                    windowsLogOns)
                                    ),
                                Restrictions.Not(Restrictions.In("Id", ids))
                            )
                    ).List<IPerson>());
            }
            return result;
        }

	    public IPerson LoadAggregate(Guid id) { return Load(id); }

		//TODO: tenant This one should be removed when tenant toggle disappear in SDK.
		public int SaveLoginAttempt(LoginAttemptModel model)
		{
			return Session.CreateSQLQuery(
					"INSERT INTO [Auditing].[Security] (Result, UserCredentials, Provider, Client, ClientIp , PersonId) VALUES (:Result, :UserCredentials, :Provider, :Client, :ClientIp, :PersonId)")
					.SetString("Result", model.Result)
					.SetString("UserCredentials", model.UserCredentials)
					.SetString("Provider", model.Provider)
					.SetString("Client", model.Client)
					.SetString("ClientIp", model.ClientIp)
					.SetGuid("PersonId", model.PersonId.GetValueOrDefault())
					.ExecuteUpdate();
		}

		public bool DoesPersonHaveExternalLogOn(DateOnly dateTime, Guid personId)
		{
			var result = Session.CreateSQLQuery(
				"exec dbo.DoesPersonHaveExternalLogOn @now=:now, @person=:person")
								.SetDateOnly("now", dateTime)
								.SetGuid("person", personId)
			                    .List();
			return result.Count > 0;
		}

		public IPerson LoadPersonAndPermissions(Guid id)
		{
			var foundPerson = Session.Get<Person>(id);

			if (foundPerson != null)
			{
				foundPerson = loadPermissionData(foundPerson);
			}
			return foundPerson;
		}
    }
}