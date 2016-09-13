using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
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

		public IList<AnalyticsBridgeTimeZonePartial> GetBridgesPartial(int timeZoneId)
		{
			AnalyticsBridgeTimeZonePartial analyticsBridgeTimeZonePartial = null;
			return _analyticsUnitOfWork.Current().Session().QueryOver<AnalyticsBridgeTimeZone>()
				.Where(b => b.TimeZoneId == timeZoneId)
				.SelectList(list => list
					.Select(x => x.DateId).WithAlias(() => analyticsBridgeTimeZonePartial.DateId)
					.Select(x => x.IntervalId).WithAlias(() => analyticsBridgeTimeZonePartial.IntervalId)
					.Select(x => x.TimeZoneId).WithAlias(() => analyticsBridgeTimeZonePartial.TimeZoneId)
					)
				.TransformUsing(Transformers.AliasToBean<AnalyticsBridgeTimeZonePartial>())
				.List<AnalyticsBridgeTimeZonePartial>();
		}

		public void Save(IList<AnalyticsBridgeTimeZone> toBeAdded)
		{
			if (!toBeAdded.Any()) return;
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