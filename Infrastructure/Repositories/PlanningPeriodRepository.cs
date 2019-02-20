using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningPeriodRepository : Repository<PlanningPeriod> , IPlanningPeriodRepository
	{
		public PlanningPeriodRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
		{
		}

		public IPlanningPeriodSuggestions Suggestions(INow now)
		{
			var uniqueSchedulePeriods = Session.GetNamedQuery("UniqueSchedulePeriods")
				.SetDateTime("date", now.UtcDateTime())
				.SetGuid("businessUnit", ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(AggregatedSchedulePeriod)))
				.List<AggregatedSchedulePeriod>();

			return new PlanningPeriodSuggestions(now, uniqueSchedulePeriods);
		}

		public IPlanningPeriodSuggestions Suggestions(INow now, ICollection<Guid> personIds)
		{
			var result = new HashSet<AggregatedSchedulePeriod>();
			foreach (var peopleBatch in personIds.Batch(1000))
			{
				Session.GetNamedQuery("UniqueSchedulePeriodsForPeople")
					.SetDateTime("date", now.UtcDateTime())
					.SetGuid("businessUnit", ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
					.SetParameterList("personIds", peopleBatch)
					.SetResultTransformer(new AliasToBeanResultTransformer(typeof(AggregatedSchedulePeriod)))
					.List<AggregatedSchedulePeriod>().ForEach(asp =>
					{
						if (result.Contains(asp))
						{
							var existing = result.First(x => x.Equals(asp));
							existing.Priority += asp.Priority;
						}
						else
						{
							result.Add(asp);
						}
					});
			}

			result.RemoveWhere(x => x.PeriodType == SchedulePeriodType.Day);
			var top10 = result.OrderByDescending(x => x.Priority).Take(10);
			return new PlanningPeriodSuggestions(now, top10.ToList());
		}

		public IEnumerable<PlanningPeriod> LoadForPlanningGroup(PlanningGroup planningGroup)
		{
			return Session.CreateCriteria(typeof(PlanningPeriod))
				.Add(Restrictions.Eq("PlanningGroup", planningGroup))
				.List<PlanningPeriod>();
		}
	}

	public class AggregatedSchedulePeriod
	{
		public SchedulePeriodType PeriodType { get; set; }
		public int Number { get; set; }
		public DateTime DateFrom { get; set; }
		public int? Culture { get; set; }
		public int Priority { get; set; }

		public override bool Equals(object obj)
		{
			var aggregatedSchedulePeriod = obj as AggregatedSchedulePeriod;
			if (aggregatedSchedulePeriod == null)
				return false;
			return PeriodType == aggregatedSchedulePeriod.PeriodType
				   && Number == aggregatedSchedulePeriod.Number
				   && DateFrom == aggregatedSchedulePeriod.DateFrom
				   && Culture == aggregatedSchedulePeriod.Culture;
		}
		public override int GetHashCode()
		{
			return $"{PeriodType}|{Number}|{DateFrom}|{Culture}".GetHashCode();
		}
	}
}