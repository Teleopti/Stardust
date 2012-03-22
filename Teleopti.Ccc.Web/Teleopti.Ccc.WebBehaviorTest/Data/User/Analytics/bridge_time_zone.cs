using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public static class bridge_time_zone
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.bridge_time_zone");
			table.Columns.Add("date_id");
			table.Columns.Add("interval_id");
			table.Columns.Add("time_zone_id");
			table.Columns.Add("local_date_id");
			table.Columns.Add("local_interval_id");
			table.Columns.Add("datasource_id");
			table.Columns.Add("insert_date");
			table.Columns.Add("update_date");
			return table;
		}

		public static void AddRow(
			this DataTable dataTable,
			int date_id,
			int interval_id,
			int time_zone_id,
			int local_date_id,
			int local_interval_id,
			int datasource_id,
			DateTime insert_date,
			DateTime update_date)
		{
			var row = dataTable.NewRow();
			row["date_id"] = date_id;
			row["interval_id"] = interval_id;
			row["time_zone_id"] = time_zone_id;
			row["local_date_id"] = local_date_id;
			row["local_interval_id"] = local_interval_id;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = insert_date;
			row["update_date"] = update_date;
			dataTable.Rows.Add(row);
		}
	}
}