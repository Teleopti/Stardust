using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Analytics;
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
			using (var session = _analyticsUnitOfWork.Current().Session().SessionFactory.OpenStatelessSession())
			using (var transaction = session.BeginTransaction())
			{
				foreach (var bridgeTimeZone in toBeAdded)
				{
					session.Insert(bridgeTimeZone);
				}
				transaction.Commit();
			}
		}
	}
}