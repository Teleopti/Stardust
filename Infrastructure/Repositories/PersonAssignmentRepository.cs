using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
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
	public class PersonAssignmentRepository : Repository<IPersonAssignment>, IPersonAssignmentRepository, IPersonAssignmentWriteSideRepository
	{
		public PersonAssignmentRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public PersonAssignmentRepository(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory)
		{
		}

		public PersonAssignmentRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{

		}

		public IPersonAssignment Load(Guid personId, DateOnly date)
		{
			throw new NotImplementedException();
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
		public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			InParameter.NotNull("persons", persons);
			var retList = new List<IPersonAssignment>();

			foreach (var personList in persons.Batch(400))
			{
				var personArray = personList.ToArray();
				var crit = personAssignmentCriteriaLoader(period, scenario);
				crit.Add(Restrictions.In("ass.Person", personArray));
				retList.AddRange(crit.List<IPersonAssignment>());
			}

			return retList;
		}

		public ICollection<IPersonAssignment> Find(DateOnlyPeriod period, IScenario scenario)
		{
			InParameter.NotNull("scenario", scenario);
			var crit = personAssignmentCriteriaLoader(period, scenario);
			using (PerformanceOutput.ForOperation("Loading personassignments"))
				return crit.List<IPersonAssignment>();
		}

		public IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario)
		{
			return Session.GetNamedQuery("fetchIdAndVersionPersonAssignment")
			              .SetEntity("scenario", scenario)
			              .SetDateTime("start", period.StartDate)
			              .SetDateTime("end", period.EndDate)
										.SetResultTransformer(
											Transformers.AliasToBeanConstructor(typeof(DateScenarioPersonId).GetConstructor(
											new[]
												{
													typeof(Guid),
													typeof(DateOnly), 
													typeof(Guid), 
													typeof(Guid),
													typeof(int)
												})))
			              .List<DateScenarioPersonId>();
		}

		private ICriteria personAssignmentCriteriaLoader(DateOnlyPeriod period, IScenario scenario)
		{
			var assCriteria = Session.CreateCriteria(typeof (PersonAssignment), "ass")
			                         .SetFetchMode("ShiftLayers", FetchMode.Join)
			                         .SetResultTransformer(Transformers.DistinctRootEntity);
			addScenarioAndFilterClauses(assCriteria, scenario, period);
			addBuClauseToNonRootQuery(assCriteria);
			return assCriteria;
		}

		private static void addBuClauseToNonRootQuery(ICriteria criteria)
		{
			if (!typeof (IAggregateRoot).IsAssignableFrom(criteria.GetRootEntityTypeIfAvailable()))
				criteria.Add(Expression.Eq("ass.BusinessUnit",
				                           ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).BusinessUnit));
		}

		private static void addScenarioAndFilterClauses(ICriteria criteria, IScenario scenario, DateOnlyPeriod period)
		{
			criteria.Add(Restrictions.Eq("ass.Scenario", scenario))
			        .Add(Restrictions.Between("Date", period.StartDate, period.EndDate));
		}

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
			IPersonAssignment ass = Session.CreateCriteria(typeof (PersonAssignment))
			                               .SetFetchMode("ShiftLayers", FetchMode.Join)
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
