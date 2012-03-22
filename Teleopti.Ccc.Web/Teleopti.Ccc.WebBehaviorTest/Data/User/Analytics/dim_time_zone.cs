using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
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

		public static void AddDimTimeZoneRow(
			this DataTable dataTable,
			int time_zone_id,
			string time_zone_code,
			string time_zone_name,
			bool default_zone,
			int utc_conversion,
			int utc_conversion_dst,
			int datasource_id,
			DateTime insert_date,
			DateTime update_date)
		{
			var row = dataTable.NewRow();
			row["time_zone_id"] = time_zone_id;
			row["time_zone_code"] = time_zone_code;
			row["time_zone_name"] = time_zone_name;
			row["default_zone"] = default_zone;
			row["utc_conversion"] = utc_conversion;
			row["utc_conversion_dst"] = utc_conversion_dst;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = insert_date;
			row["update_date"] = update_date;
			row["to_be_deleted"] = false;
			dataTable.Rows.Add(row);
		}

	}
}