using System;
using System.Collections.Generic;
using NHibernate.Transform;
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
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT 
						date_id {nameof(AnalyticsBridgeTimeZone.DateId)},
						interval_id {nameof(AnalyticsBridgeTimeZone.IntervalId)},
						time_zone_id {nameof(AnalyticsBridgeTimeZone.TimeZoneId)},
						local_date_id {nameof(AnalyticsBridgeTimeZone.LocalDateId)},
						local_interval_id {nameof(AnalyticsBridgeTimeZone.LocalIntervalId)}
						").SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsBridgeTimeZone)))
				.SetReadOnly(true)
				.List<AnalyticsBridgeTimeZone>();
		}

		public void Save(List<AnalyticsBridgeTimeZone> toBeAdded)
		{
			foreach (var bridge in toBeAdded)
			{
				_analyticsUnitOfWork.Current().Session().CreateSQLQuery(
					$@"INSERT INTO [mart].[bridge_time_zone]
							([date_id]
							,[interval_id]
							,[time_zone_id]
							,[local_date_id]
							,[local_interval_id]
							,[datasource_id]
							,[insert_date]
							,[update_date])
						VALUES
							(:date_id
							,:interval_id
							,:time_zone_id
							,:local_date_id
							,:local_interval_id
							,:datasource_id
							,:insert_date
							,:update_date)")
						.SetParameter(":date_id", bridge.DateId)
						.SetParameter(":interval_id", bridge.IntervalId)
						.SetParameter(":time_zone_id", bridge.TimeZoneId)
						.SetParameter(":local_date_id", bridge.LocalDateId)
						.SetParameter(":local_interval_id", bridge.LocalIntervalId)
						.SetParameter(":datasource_id", 1)
						.SetParameter(":insert_date", DateTime.UtcNow)
						.SetParameter(":update_date", DateTime.UtcNow)
						.ExecuteUpdate();
			}
		}
	}
}