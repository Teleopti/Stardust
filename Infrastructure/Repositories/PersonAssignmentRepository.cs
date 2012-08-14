using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    ///<summary>
    /// Repository for PersonAssignment aggregate
    ///</summary>
    public class PersonAssignmentRepository : Repository<IPersonAssignment>, IPersonAssignmentRepository
    {
        public PersonAssignmentRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public PersonAssignmentRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

        /// <summary>
        /// Finds the specified persons.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-03-06
        /// </remarks>
        public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons,
                                               DateTimePeriod period,
                                               IScenario scenario)
        {
            InParameter.NotNull("persons", persons);
            var retList = new List<IPersonAssignment>();
            //var multi = Session.CreateMultiCriteria();

            foreach (var personList in persons.Batch(400))
            {
				var multi = Session.CreateMultiCriteria();
                var personArray = personList.ToArray();
                var criterias = personAssignmentCriteriaLoader(period, scenario);
                foreach (var criteria in criterias)
                {
                    criteria.Add(Restrictions.In("ass.Person", personArray));
                    multi.Add(criteria);
                }
                var result = multi.List();
                retList.AddRange(CollectionHelper.ToDistinctGenericCollection<IPersonAssignment>(result[0]));
            }

            return retList;
        }

        public ICollection<IPersonAssignment> Find(DateTimePeriod period, IScenario scenario)
        {
            InParameter.NotNull("scenario", scenario);
            var multi = Session.CreateMultiCriteria();
            personAssignmentCriteriaLoader(period, scenario).ForEach(c=>multi.Add(c));
			using(PerformanceOutput.ForOperation("Loading personassignments"))
            return CollectionHelper.ToDistinctGenericCollection<IPersonAssignment>(multi.List()[0]);
        }

        private IEnumerable<ICriteria> personAssignmentCriteriaLoader(DateTimePeriod period, IScenario scenario)
        {
            var assWithMain = Session.CreateCriteria(typeof(PersonAssignment), "ass")
                        .SetFetchMode("MainShift", FetchMode.Join)
                        .SetFetchMode("MainShift.LayerCollection", FetchMode.Join);


            var assWithPers = Session.CreateCriteria(typeof(PersonAssignment), "ass")
                    .SetFetchMode("PersonalShiftCollection", FetchMode.Join);

            var persWithLayers = Session.CreateCriteria(typeof(PersonalShift))
                        .CreateAlias("Parent", "ass")
                        .SetFetchMode("LayerCollection", FetchMode.Join);

            var assWithOvertime = Session.CreateCriteria(typeof(PersonAssignment), "ass")
                    .SetFetchMode("OvertimeShiftCollection", FetchMode.Join);

            var overWithLayers = Session.CreateCriteria(typeof(OvertimeShift))
                        .CreateAlias("Parent", "ass")
                        .SetFetchMode("LayerCollection", FetchMode.Join);

            var ret = new[] { assWithMain, assWithPers, persWithLayers, assWithOvertime, overWithLayers };
            ret.ForEach(crit => addScenarioAndFilterClauses(crit, scenario, period));
            ret.ForEach(addBuClauseToNonRootQuery);
            return ret;
        }

        private static void addBuClauseToNonRootQuery(ICriteria criteria)
        {
            if(!typeof(IAggregateRoot).IsAssignableFrom(criteria.GetRootEntityTypeIfAvailable()))
                criteria.Add(Expression.Eq("ass.BusinessUnit",
                                           ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).BusinessUnit));
        }

        private static void addScenarioAndFilterClauses(ICriteria criteria, IScenario scenario, DateTimePeriod period)
        {
            criteria.Add(Restrictions.Eq("ass.Scenario", scenario))
				.Add(Restrictions.Gt("ass.Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Lt("ass.Period.period.Minimum", period.EndDateTime));
        }


        #region IPersonAssignmentRepository Members

        /// <summary>
        /// Removes the main shift from the data source.
        /// </summary>
        /// <param name="personAssignment">The person assignment.</param>
        /// <remarks>
        /// Tested from Domain test.
        /// </remarks>
        void IPersonAssignmentRepository.RemoveMainShift(IPersonAssignment personAssignment)
        {
			  if(!Session.Contains(personAssignment))
				  Session.Lock(personAssignment, LockMode.None);
            Session.Delete(personAssignment.MainShift);
        }

        public IEnumerable<IPersonAssignment> Find(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence, IPersonAbsenceRepository repository)
        {
            IList<IPerson> persons = new List<IPerson> { person };
            List<IPersonAssignment> ret = new List<IPersonAssignment>();
            ICollection<DateTimePeriod> searchPeriods = repository.AffectedPeriods(person, scenario, period, absence);
            foreach (var p in searchPeriods)
            {
                ret.AddRange(Find(persons, p, scenario));
            }
            return ret;
        }
        public IEnumerable<IPersonAssignment> Find(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence)
        {
            return Find(period, scenario, person, absence, new PersonAbsenceRepository(UnitOfWork));
        }

        #endregion

        /// <summary>
        /// Loads the aggregate.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-12
        /// </remarks>
        public IPersonAssignment LoadAggregate(Guid id)
        {
            IPersonAssignment ass = Session.CreateCriteria(typeof(PersonAssignment))
                        .SetFetchMode("PersonalShiftCollection", FetchMode.Join)
                        .SetFetchMode("PersonalShiftCollection.LayerCollection", FetchMode.Join)
                        .SetFetchMode("MainShift", FetchMode.Join)
                        .SetFetchMode("MainShift.LayerCollection", FetchMode.Join)
                        .SetFetchMode("OvertimeShiftCollection", FetchMode.Join)
                        .SetFetchMode("OvertimeShiftCollection.LayerCollection", FetchMode.Join)
                        .Add(Restrictions.Eq("Id", id))
                        .UniqueResult<IPersonAssignment>();
            if (ass != null)
            {
                var initializer = new InitializeRootsPersonAssignment(new List<IPersonAssignment> {ass});
                initializer.Initialize();   
            }
                
            return ass;
        }
	}
}
