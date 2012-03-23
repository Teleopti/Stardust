using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class FillBridgeTimeZoneFromData : IAnalyticsDataSetup
	{
		private readonly IDateData _dates;
		private readonly IIntervalData _intervals;
		private readonly ITimeZoneData _timeZones;
		private readonly IDatasourceData _datasource;

		public FillBridgeTimeZoneFromData(IDateData dates, IIntervalData intervals, ITimeZoneData timeZones, IDatasourceData datasource)
		{
			_dates = dates;
			_intervals = intervals;
			_timeZones = timeZones;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
		{
			var dim_date = _dates.Table.AsEnumerable();
			var dim_interval = _intervals.Table.AsEnumerable();
			var dim_time_zone = _timeZones.Table.AsEnumerable();

			var query = from d in dim_date
			            let date_id = (int) d["date_id"]
			            from i in dim_interval
			            let interval_id = (int) i["interval_id"]
			            from z in dim_time_zone
			            let time_zone_id = (int) z["time_zone_id"]
			            let timeZone = TimeZoneInfo.FindSystemTimeZoneById(z["time_zone_code"].ToString())
			            let date = (DateTime) d["date_date"]
			            let time = (DateTime) i["interval_start"]
			            let dateTime = date.Date + time.TimeOfDay
			            let localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone)
						let local_dates = dim_date.FindDateIdsByDate(localDateTime)
			            where local_dates.Any()
			            let local_date_id = local_dates.Single()
						let local_interval_id = dim_interval.FindIntervalIdByTimeOfDay(localDateTime)
			            select new
			                   	{
			                   		date_id,
			                   		interval_id,
			                   		time_zone_id,
			                   		local_date_id,
			                   		local_interval_id
			                   	};

			var table = bridge_time_zone.CreateTable();

			query.ForEach(
				a => table.AddTimeZone(
					a.date_id,
					a.interval_id,
					a.time_zone_id,
					a.local_date_id,
					a.local_interval_id,
					_datasource.RaptorDefaultDatasourceId)
				);

			Bulk.Insert(connection, table);
		}
	}

}