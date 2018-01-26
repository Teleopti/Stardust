using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceDataRepository : Repository<IExternalPerformanceData>, IExternalPerformanceDataRepository
	{
		public ExternalPerformanceDataRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public ICollection<IExternalPerformanceData> FindByPeriod(DateTimePeriod period)
		{
			return Session.CreateCriteria<ExternalPerformanceData>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Between("DateFrom", period.StartDateTime, period.EndDateTime)))
				.List<IExternalPerformanceData>();
		}

		public ICollection<IExternalPerformanceData> Find(DateTime date, List<Guid> personIds, int performanceId)
		{
			var performance = Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Eq("ExternalId", performanceId)))
				.UniqueResult<IExternalPerformance>();

			return Session.CreateCriteria<ExternalPerformanceData>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Eq("DateFrom", date))
					.Add(Restrictions.In("PersonId", personIds))
					.Add(Restrictions.Eq("ExternalPerformance", performance))
				)
				.List<IExternalPerformanceData>();
		}

		public ICollection<Guid> FindPersonsCouldGetBadge(DateTime date, List<Guid> personIds, int performanceId, double badgeThreshold)
		{
			var performance = Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Eq("ExternalId", performanceId)))
				.UniqueResult<IExternalPerformance>();

			var performanceData = Session.CreateCriteria<ExternalPerformanceData>()
				.Add(Restrictions.Conjunction()
					.Add(Restrictions.Eq("DateFrom", date))
					.Add(Restrictions.In("PersonId", personIds))
					.Add(Restrictions.Eq("ExternalPerformance", performance))
					.Add(Restrictions.Ge("Score", badgeThreshold))
				)
				.List<IExternalPerformanceData>();

			return performanceData.Select(x => x.PersonId).ToList();
		}
	}
}
