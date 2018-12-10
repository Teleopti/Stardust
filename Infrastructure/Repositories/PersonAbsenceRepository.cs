using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	///<summary>
	/// Repository for PersonAssignment aggregate
	///</summary>
	public class PersonAbsenceRepository : Repository<IPersonAbsence>, IPersonAbsenceRepository
	{
		public PersonAbsenceRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
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
		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons,
			DateTimePeriod period,
			IScenario scenario)
		{
			InParameter.NotNull(nameof(persons), persons);
			InParameter.NotNull(nameof(period), period);
			InParameter.NotNull(nameof(scenario), scenario);
			var retList = new List<IPersonAbsence>();

			var restrictions = Restrictions.Conjunction()
				.Add(Restrictions.Gt("Layer.Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Lt("Layer.Period.period.Minimum", period.EndDateTime));

			if (scenario != null)
				restrictions.Add(Restrictions.Eq("Scenario", scenario));

			foreach (var personList in persons.Batch(400))
			{
				var people = personList.ToArray();

				retList.AddRange(Session.CreateCriteria(typeof(PersonAbsence), "abs")
					.Add(restrictions)
					.Add(Restrictions.InG("Person", people))
					.SetResultTransformer(Transformers.DistinctRootEntity)
					.List<IPersonAbsence>());
			}

			initializeAbsences(retList);
			return retList;

		}

		public ICollection<IPersonAbsence> FindExact(IPerson person, DateTimePeriod period, IAbsence absence,
			IScenario scenario)
		{
			var retList = Session.CreateCriteria(typeof(PersonAbsence))
				.Add(Restrictions.Eq("Layer.Period.period.Minimum", period.StartDateTime))
				.Add(Restrictions.Eq("Layer.Period.period.Maximum", period.EndDateTime))
				.Add(Restrictions.Eq("Scenario", scenario))
				.Add(Restrictions.Eq("Layer.Payload", absence))
				.Add(Restrictions.Eq("Person", person))
				.List<IPersonAbsence>();

			initializeAbsences(retList);
			return retList;
		}

		public bool IsThereScheduledAgents(Guid businessUnitId, DateOnlyPeriod period)
		{
			var startDate = period.StartDate.Date;
			var endDate = period.EndDate.Date;

			var sql = $@"IF EXISTS (SELECT TOP 1 pa.Id
  FROM PersonAbsence pa
 INNER JOIN Person p ON pa.Person = p.Id
 INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
 INNER JOIN Team t ON pp.Team = t.Id
 INNER JOIN [Site] s ON t.[Site] = s.Id
 INNER JOIN BusinessUnit bu ON s.BusinessUnit = bu.Id
 WHERE bu.Id = :{nameof(businessUnitId)}
   AND ((pa.Minimum >= :{nameof(startDate)} AND pa.Minimum <= :{nameof(endDate)})
    OR (pa.Maximum >= :{nameof(startDate)} AND pa.Maximum <= :{nameof(endDate)})
    OR (pa.Minimum <= :{nameof(startDate)} AND pa.Maximum >= :{nameof(endDate)})))
SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)";
			var result = Session.CreateSQLQuery(sql)
				.SetParameter(nameof(businessUnitId), businessUnitId)
				.SetDateTime(nameof(startDate), startDate)
				.SetDateTime(nameof(endDate), endDate)
				.UniqueResult<bool>();
			return result;
		}

		public ICollection<IPersonAbsence> Find(IEnumerable<Guid> personAbsenceIds, IScenario scenario)
		{
			InParameter.NotNull(nameof(personAbsenceIds), personAbsenceIds);

			var retList = new List<IPersonAbsence>();

			foreach (var personAbsenceIdList in personAbsenceIds.Batch(400))
			{
				var absenceIdList = personAbsenceIdList.ToArray();
				retList.AddRange(Session.CreateCriteria(typeof(PersonAbsence), "abs")
					.Add(Restrictions.InG("Id", absenceIdList))
					.Add(Restrictions.Eq("Scenario", scenario))
					.SetResultTransformer(Transformers.DistinctRootEntity)
					.List<IPersonAbsence>());
			}

			initializeAbsences(retList);
			return retList;
		}

		public ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period,
			IAbsence absence = null)
		{
			var criteria = Session.CreateCriteria(typeof(PersonAbsence))
				.SetProjection(Projections.Property("Layer.Period"))
				.Add(Restrictions.Gt("Layer.Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Lt("Layer.Period.period.Minimum", period.EndDateTime))
				.Add(Restrictions.Eq("Scenario", scenario))
				.Add(Restrictions.Eq("Person", person));
			if (absence != null)
			{
				criteria.Add(Restrictions.Eq("Layer.Payload", absence));
			}

			return criteria.List<DateTimePeriod>();
		}


		/// <summary>
		/// Finds the specified period.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="scenario">The scenario.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2007-11-12
		/// </remarks>
		public ICollection<IPersonAbsence> Find(DateTimePeriod period, IScenario scenario)
		{
			var restrictions = Restrictions.Conjunction()
				.Add(Restrictions.Gt("Layer.Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Lt("Layer.Period.period.Minimum", period.EndDateTime));

			if (scenario != null)
				restrictions.Add(Restrictions.Eq("Scenario", scenario));

			var retList = Session.CreateCriteria(typeof(PersonAbsence), "abs")
				.Add(restrictions)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IPersonAbsence>();

			initializeAbsences(retList);
			return retList;
		}

		private static void initializeAbsences(IEnumerable<IPersonAbsence> personAbsences)
		{
			foreach (var personAbsence in personAbsences)
			{
				if (!LazyLoadingManager.IsInitialized(personAbsence.Layer.Payload))
					LazyLoadingManager.Initialize(personAbsence.Layer.Payload);
			}
		}

		public IPersonAbsence LoadAggregate(Guid id)
		{
			var retObj = Session.CreateCriteria(typeof(PersonAbsence))
				.Add(Restrictions.IdEq(id))
				.UniqueResult<PersonAbsence>();

			if (retObj != null)
			{
				var initializer = new InitializeRootsPersonAbsence(new List<IPersonAbsence> {retObj});
				initializer.Initialize();
			}

			return retObj;
		}

	}
}
