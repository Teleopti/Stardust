using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsBridgeTimeZoneRepository : IAnalyticsBridgeTimeZoneRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsBridgeTimeZoneRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<AnalyticsBridgeTimeZone> GetBridges(int timeZoneId)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsBridgeTimeZone>()
				.Add(Restrictions.Eq(nameof(AnalyticsBridgeTimeZone.TimeZoneId), timeZoneId));

			var result = query.List<AnalyticsBridgeTimeZone>();
			return result;
		}

		public void Save(IList<AnalyticsBridgeTimeZone> toBeAdded)
		{
			foreach (var bridgeTimeZone in toBeAdded)
			{
				_analyticsUnitOfWork.Current().Session().Save(bridgeTimeZone);
			}
		}
	}
}