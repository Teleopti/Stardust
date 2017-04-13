using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningPeriodRepository : Repository<IPlanningPeriod> , IPlanningPeriodRepository
	{
		public PlanningPeriodRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IPlanningPeriodSuggestions Suggestions(INow now)
		{
			var uniqueSchedulePeriods = Session.GetNamedQuery("UniqueSchedulePeriods")
				.SetDateTime("date", now.UtcDateTime())
				.SetGuid("businessUnit", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(AggregatedSchedulePeriod)))
				.List<AggregatedSchedulePeriod>();

			return new PlanningPeriodSuggestions(now, uniqueSchedulePeriods);
		}

		public IEnumerable<IPlanningPeriod> LoadForAgentGroup(IAgentGroup agentGroup)
		{
			return Session.CreateCriteria(typeof(PlanningPeriod))
				.Add(Restrictions.Eq("AgentGroup", agentGroup))
				.List<IPlanningPeriod>();
		}
	}

	public struct AggregatedSchedulePeriod
	{
		public SchedulePeriodType PeriodType { get; set; }
		public int Number { get; set; }
		public DateTime DateFrom { get; set; }
		public int? Culture { get; set; }
		public int Priority { get; set; }
	}
}