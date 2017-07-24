using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsBridgeTimeZoneRepository : IAnalyticsBridgeTimeZoneRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsBridgeTimeZoneRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<AnalyticsBridgeTimeZonePartial> GetBridgesPartial(int timeZoneId, int minDateId)
		{
			AnalyticsBridgeTimeZonePartial analyticsBridgeTimeZonePartial = null;
			return _analyticsUnitOfWork.Current().Session().QueryOver<AnalyticsBridgeTimeZone>()
				.Where(b => b.TimeZoneId == timeZoneId)
				.And(x => x.DateId >= minDateId)
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
			using (var transaction = session.Connection.BeginTransaction())
			{
				var dt = new DataTable();
				dt.Columns.Add("date_id", typeof(int));
				dt.Columns.Add("interval_id", typeof(short));
				dt.Columns.Add("time_zone_id", typeof(short));
				dt.Columns.Add("local_date_id", typeof(int));
				dt.Columns.Add("local_interval_id", typeof(short));
				dt.Columns.Add("datasource_id", typeof(int));
				dt.Columns.Add("insert_date", typeof(DateTime));
				dt.Columns.Add("update_date", typeof(DateTime));
				
				foreach (var skill in toBeAdded)
				{
					var row = dt.NewRow();
					row["date_id"] = skill.DateId;
					row["interval_id"] = skill.IntervalId;
					row["time_zone_id"] = skill.TimeZoneId;
					row["local_date_id"] = skill.LocalDateId;
					row["local_interval_id"] = skill.LocalIntervalId;
					row["datasource_id"] = 1;
					row["insert_date"] = DateTime.UtcNow;
					row["update_date"] = DateTime.UtcNow;
					dt.Rows.Add(row);
				}
				
				using (var sqlBulkCopy = new SqlBulkCopy((SqlConnection) session.Connection, SqlBulkCopyOptions.Default, (SqlTransaction) transaction))
				{
					sqlBulkCopy.DestinationTableName = "[mart].[bridge_time_zone]";
					sqlBulkCopy.WriteToServer(dt);
				}
				
				transaction.Commit();
			}
		}

		public int GetMaxDateForTimeZone(int timeZoneId)
		{
			return _analyticsUnitOfWork.Current().Session().QueryOver<AnalyticsBridgeTimeZone>()
				.Where(b => b.TimeZoneId == timeZoneId)
				.Select(x => x.DateId)
				.OrderBy(zone => zone.DateId)
				.Desc
				.Take(1)
				.SingleOrDefault<int>();
		}
	}
}