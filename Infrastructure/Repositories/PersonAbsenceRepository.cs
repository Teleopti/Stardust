using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	///<summary>
	/// Repository for PersonAssignment aggregate
	///</summary>
	public class PersonAbsenceRepository : Repository<IPersonAbsence>, IPersonAbsenceRepository
	{
		public PersonAbsenceRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public PersonAbsenceRepository(IUnitOfWorkFactory unitOfWorkFactory)
#pragma warning disable 618
			: base(unitOfWorkFactory)
#pragma warning restore 618
		{
		}

		public PersonAbsenceRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{

		}

		/// <summary>
		/// Finds the specified PersonAbsence.
		/// </summary>
		/// <param name="persons">The persons.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons,
						      DateTimePeriod period)
		{
			InParameter.NotNull("persons", persons);

			ICollection<IPersonAbsence> retList = Session.CreateCriteria(typeof(PersonAbsence), "abs")
							.Add(Subqueries.Exists(GetAgentAbsencesInPeriod(period)
							.Add(Restrictions.In("Person", new List<IPerson>(persons)))))
							.SetResultTransformer(Transformers.DistinctRootEntity)
							.List<IPersonAbsence>();
			initializeAbsences(retList);
			return retList;

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
			InParameter.NotNull("persons", persons);
			InParameter.NotNull("period", period);
			InParameter.NotNull("scenario", scenario);
			var retList = new List<IPersonAbsence>();

			foreach (var personList in persons.Batch(400))
			{

				retList.AddRange(Session.CreateCriteria(typeof(PersonAbsence), "abs")
				    .Add(Subqueries.Exists(GetAgentAbsencesInPeriod(period, scenario)
							       .Add(Restrictions.In("Person", personList.ToArray()))))
				    .SetResultTransformer(Transformers.DistinctRootEntity)
				    .List<IPersonAbsence>());
			}

			initializeAbsences(retList);
			return retList;

		}

		public ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence)
		{
			const string q = @"select pa.Layer.Period
                            from PersonAbsence pa
                            where pa.Person=:person 
                                and pa.Layer.Period.period.Maximum > :startTime
                                and pa.Layer.Period.period.Minimum < :endTime
                                and pa.Scenario =:scenario
                                and pa.Layer.Payload =:absence
								ORDER BY pa.Layer.Period.period.Minimum";

			IList<DateTimePeriod> periods = Session.CreateQuery(q)
						.SetEntity("person", person)
						.SetDateTime("startTime", period.StartDateTime)
						.SetDateTime("endTime", period.EndDateTime)
						.SetEntity("scenario", scenario)
						.SetEntity("absence", absence)
						.List<DateTimePeriod>();
			return periods;
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
			ICollection<IPersonAbsence> retList = Session.CreateCriteria(typeof(PersonAbsence), "abs")
						.Add(Subqueries.Exists(GetAgentAbsencesInPeriod(period, scenario)))
						.SetResultTransformer(Transformers.DistinctRootEntity)
						.List<IPersonAbsence>();
			initializeAbsences(retList);
			return retList;
		}

		private static void initializeAbsences(IEnumerable<IPersonAbsence> personAbsences)
		{
			foreach (IPersonAbsence personAbsence in personAbsences)
			{
				if (!LazyLoadingManager.IsInitialized(personAbsence.Layer.Payload))
					LazyLoadingManager.Initialize(personAbsence.Layer.Payload);
			}
		}

		private static DetachedCriteria GetAgentAbsencesInPeriod(DateTimePeriod period)
		{
			return GetAgentAbsencesInPeriod(period, null);
		}

		private static DetachedCriteria GetAgentAbsencesInPeriod(DateTimePeriod period, IScenario scenario)
		{
			DetachedCriteria dCrit = DetachedCriteria.For<PersonAbsence>()
			    .SetProjection(Projections.Property("Id"))
			    .Add(Property.ForName("Id").EqProperty("abs.Id"))
			    .Add(Restrictions.Gt("Layer.Period.period.Maximum", period.StartDateTime))
			    .Add(Restrictions.Lt("Layer.Period.period.Minimum", period.EndDateTime));

			if (scenario != null)
				dCrit.Add(Restrictions.Eq("Scenario", scenario));
			return dCrit;
		}

		public IPersonAbsence LoadAggregate(Guid id)
		{
			PersonAbsence retObj = Session.CreateCriteria(typeof(PersonAbsence))
				    .Add(Restrictions.IdEq(id))
				    .UniqueResult<PersonAbsence>();
			if (retObj != null)
			{
				var initializer = new InitializeRootsPersonAbsence(new List<IPersonAbsence> { retObj });
				initializer.Initialize();
			}
			return retObj;
		}

	}
}
