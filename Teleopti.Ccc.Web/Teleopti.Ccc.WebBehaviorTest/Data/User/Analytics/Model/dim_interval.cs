using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Model
{
	public static class dim_interval
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_interval");
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("interval_name");
			table.Columns.Add("halfhour_name");
			table.Columns.Add("hour_name");
			table.Columns.Add("interval_start", typeof (DateTime));
			table.Columns.Add("interval_end", typeof(DateTime));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date");
			table.Columns.Add("update_date");
			return table;
		}

		public static void AddRow(
			this DataTable dataTable,
			int interval_id,
			string interval_name,
			string halfhour_name,
			string hour_name,
			DateTime interval_start,
			DateTime interval_end,
			int datasource_id,
			DateTime insert_date,
			DateTime update_date)
		{
			var row = dataTable.NewRow();
			row["interval_id"] = interval_id;
			row["interval_name"] = interval_name;
			row["halfhour_name"] = halfhour_name;
			row["hour_name"] = hour_name;
			row["interval_start"] = interval_start;
			row["interval_end"] = interval_end;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = insert_date;
			row["update_date"] = update_date;
			dataTable.Rows.Add(row);
		}
	}
}