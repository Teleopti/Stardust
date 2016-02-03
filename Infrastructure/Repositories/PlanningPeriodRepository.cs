using System;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningPeriodRepository : Repository<IPlanningPeriod> , IPlanningPeriodRepository
	{
#pragma warning disable 618
		public PlanningPeriodRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

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