using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FillBridgeTimeZoneFromData : IAnalyticsDataSetup, IBridgeTimeZone
	{
		private readonly IDatasourceData _datasource;
		private readonly IDateData _dates;
		private readonly IIntervalData _intervals;
		private readonly ITimeZoneData _timeZones;

		public FillBridgeTimeZoneFromData(IDateData dates, IIntervalData intervals, ITimeZoneData timeZones,
			IDatasourceData datasource)
		{
			_dates = dates;
			_intervals = intervals;
			_timeZones = timeZones;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var dim_date = _dates.Rows.AsEnumerable();
			var dim_interval = _intervals.Rows.AsEnumerable();
			var dim_time_zone = _timeZones.Rows.AsEnumerable();

			var table = bridge_time_zone.CreateTable();

			foreach (var dateRow in dim_date)
			{
				foreach (var intervalRow in dim_interval)
				{
					foreach (var timeZoneRow in dim_time_zone)
					{
						var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneRow["time_zone_code"].ToString());
						var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(((DateTime)dateRow["date_date"]).Add(((DateTime)intervalRow["interval_start"]).TimeOfDay), timeZone);
						var local_dates = dim_date.FindDateIdsByDate(localDateTime);
						if (!local_dates.Any())
						{
							continue;
						}
						var local_interval_id = dim_interval.FindIntervalIdByTimeOfDay(localDateTime);

						table.AddTimeZone(
							(int) dateRow["date_id"],
							(int) intervalRow["interval_id"],
							(int) timeZoneRow["time_zone_id"],
							local_dates.Single(),
							local_interval_id,
							_datasource.RaptorDefaultDatasourceId);
					}
				}
			}

			Bulk.Insert(connection, table);

			Rows = table.AsEnumerable();
		}

		public IEnumerable<DataRow> Rows { get; set; }
	}
}