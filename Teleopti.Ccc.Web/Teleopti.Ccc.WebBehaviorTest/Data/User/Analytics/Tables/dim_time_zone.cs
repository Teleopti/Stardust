using System;
using System.Data;
using System.Linq;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables
{
	public static class dim_time_zone
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_time_zone");
			table.Columns.Add("time_zone_id", typeof(int));
			table.Columns.Add("time_zone_code");
			table.Columns.Add("time_zone_name");
			table.Columns.Add("default_zone", typeof(bool));
			table.Columns.Add("utc_conversion", typeof(int));
			table.Columns.Add("utc_conversion_dst", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date");
			table.Columns.Add("update_date");
			table.Columns.Add("to_be_deleted", typeof(bool));
			//table.Columns.Add("only_one_default_zone");
			return table;
		}

		public static void AddTimeZone(
			this DataTable dataTable,
			int time_zone_id,
			string time_zone_code,
			string time_zone_name,
			bool default_zone,
			int utc_conversion,
			int utc_conversion_dst,
			int datasource_id)
		{
			var row = dataTable.NewRow();
			row["time_zone_id"] = time_zone_id;
			row["time_zone_code"] = time_zone_code;
			row["time_zone_name"] = time_zone_name;
			row["default_zone"] = default_zone;
			row["utc_conversion"] = utc_conversion;
			row["utc_conversion_dst"] = utc_conversion_dst;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["to_be_deleted"] = false;
			dataTable.Rows.Add(row);
		}

		public static TimeZoneInfo FindTimeZoneById(
			this DataTable dataTable,
			int time_zone_id)
		{
			var time_zone_code =
				(
					from t in dataTable.AsEnumerable()
					where (int) t["time_zone_id"] == time_zone_id
					select (string) t["time_zone_code"]
				).Single();
			return TimeZoneInfo.FindSystemTimeZoneById(time_zone_code);
		}

	}
}