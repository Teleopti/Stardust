using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class bridge_time_zone
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.bridge_time_zone");
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("time_zone_id", typeof(int));
			table.Columns.Add("local_date_id", typeof(int));
			table.Columns.Add("local_interval_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			return table;
		}

		public static void AddTimeZone(
			this DataTable dataTable,
			int date_id,
			int interval_id,
			int time_zone_id,
			int local_date_id,
			int local_interval_id,
			int datasource_id)
		{
			var row = dataTable.NewRow();
			row["date_id"] = date_id;
			row["interval_id"] = interval_id;
			row["time_zone_id"] = time_zone_id;
			row["local_date_id"] = local_date_id;
			row["local_interval_id"] = local_interval_id;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}

		public static IEnumerable<DataRow> FindBridgeTimeZoneRowsByIds(
			this IEnumerable<DataRow> rows,
			int date_id,
			int interval_id,
			int time_zone_id)
		{
			return (from b in rows
			        where
			        	(int) b["date_id"] == date_id &&
			        	(int) b["interval_id"] == interval_id &&
			        	(int) b["time_zone_id"] == time_zone_id
			        select
			        	b).ToArray();
		}
	}
}