using System;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningPeriodRepository : Repository<IPlanningPeriod> , IPlanningPeriodRepository
	{
		public PlanningPeriodRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public PlanningPeriodRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public PlanningPeriodRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IPlanningPeriodSuggestions Suggestions(INow now)
		{
			var uniqueSchedulePeriods = Session.GetNamedQuery("UniqueSchedulePeriods")
				.SetDateTime("date", now.UtcDateTime())
				.SetGuid("businessUnit", CurrentBusinessUnit.InstanceForEntities.Current().Id.GetValueOrDefault())
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(AggregatedSchedulePeriod)))
				.List<AggregatedSchedulePeriod>();

			return new PlanningPeriodSuggestions(now, uniqueSchedulePeriods);
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