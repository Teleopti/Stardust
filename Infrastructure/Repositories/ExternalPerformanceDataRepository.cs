using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceDataRepository : Repository<IExternalPerformanceData>, IExternalPerformanceDataRepository
	{
		private readonly IBusinessUnitRepository _businessUnitRepository;
		
		public ExternalPerformanceDataRepository(ICurrentUnitOfWork currentUnitOfWork, IBusinessUnitRepository businessUnitRepository) : base(currentUnitOfWork, null, null)
		{
			_businessUnitRepository = businessUnitRepository;
		}

		public ICollection<IExternalPerformanceData> FindByPeriod(DateOnlyPeriod period)
		{
			var businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current();
			return Session.CreateCriteria<ExternalPerformanceData>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Eq("BusinessUnit", businessUnit))
					.Add(Restrictions.Between("DateFrom", period.StartDate, period.EndDate)))
				.List<IExternalPerformanceData>();
		}

		public ICollection<IExternalPerformanceData> FindPersonsCouldGetBadgeOverThreshold(DateOnly date, List<Guid> personIds, int performanceId, double badgeThreshold, Guid businessId)
		{
			var businessUnit = _businessUnitRepository.Get(businessId);
			if (businessUnit == null) return new List<IExternalPerformanceData>();
			
			var performance = Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Eq("BusinessUnit", businessUnit))
					.Add(Restrictions.Eq("ExternalId", performanceId)))
				.UniqueResult<IExternalPerformance>();

			var performanceData = new List<IExternalPerformanceData>();
			foreach (var batch in personIds.Batch(500))
			{
				performanceData.AddRange(Session.CreateCriteria<ExternalPerformanceData>()
					.Add(Restrictions.Conjunction()
						.Add(Restrictions.Eq("BusinessUnit", businessUnit))
						.Add(Restrictions.Eq("DateFrom", date))
						.Add(Restrictions.In("PersonId", batch.ToArray()))
						.Add(Restrictions.Eq("ExternalPerformance", performance))
						.Add(Restrictions.Ge("Score", badgeThreshold))
					)
					.List<IExternalPerformanceData>()
				);
			}

			return performanceData;
		}
	}
}
