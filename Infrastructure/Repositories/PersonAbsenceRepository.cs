using System;
using System.Collections.Generic;
using System.Linq;
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
		public ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons, DateTimePeriod period)
		{
			InParameter.NotNull("persons", persons);

			var restrictions = Restrictions.Conjunction().Add(Restrictions.Gt("Layer.Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Lt("Layer.Period.period.Minimum", period.EndDateTime));

			ICollection<IPersonAbsence> retList = Session.CreateCriteria(typeof(PersonAbsence), "abs")
							.Add(restrictions)
							.Add(Restrictions.InG("Person", new List<IPerson>(persons)))
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

			var restrictions = Restrictions.Conjunction().Add(Restrictions.Gt("Layer.Period.period.Maximum", period.StartDateTime))
				.Add(Restrictions.Lt("Layer.Period.period.Minimum", period.EndDateTime));

			if (scenario != null)
				restrictions.Add(Restrictions.Eq("Scenario", scenario));

			foreach (var personList in persons.Batch(400))
			{
				retList.AddRange(Session.CreateCriteria(typeof(PersonAbsence), "abs")
				    .Add(restrictions)
					.Add(Restrictions.InG("Person", personList.ToArray()))
				    .SetResultTransformer(Transformers.DistinctRootEntity)
				    .List<IPersonAbsence>());
			}

			initializeAbsences(retList);
			return retList;

		}

		public ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence = null)
		{
			var criteria = Session.CreateCriteria(typeof (PersonAbsence))
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
			var restrictions = Restrictions.Conjunction().Add(Restrictions.Gt("Layer.Period.period.Maximum", period.StartDateTime))
			    .Add(Restrictions.Lt("Layer.Period.period.Minimum", period.EndDateTime));

			if (scenario != null)
				restrictions.Add(Restrictions.Eq("Scenario", scenario));

			ICollection<IPersonAbsence> retList = Session.CreateCriteria(typeof(PersonAbsence), "abs")
						.Add(restrictions)
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
