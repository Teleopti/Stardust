using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables
{
	public static class dim_date
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_date");
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("date_date", typeof(DateTime));
			table.Columns.Add("year", typeof(int));
			table.Columns.Add("year_month", typeof(int));
			table.Columns.Add("month", typeof(int));
			table.Columns.Add("month_name");
			table.Columns.Add("month_resource_key");
			table.Columns.Add("day_in_month", typeof(int));
			table.Columns.Add("weekday_number", typeof(int));
			table.Columns.Add("weekday_name");
			table.Columns.Add("weekday_resource_key");
			table.Columns.Add("week_number", typeof(int));
			table.Columns.Add("year_week");
			table.Columns.Add("quarter");
			table.Columns.Add("insert_date");
			return table;
		}

		public static void AddRow(
			this DataTable dataTable,
			int date_id,
			DateTime date_date, 
			int year,
			int year_month,
			int month,
			string month_name,
			string month_resource_key,
			int day_in_month,
			int weekday_number,
			string weekday_name,
			string weekday_resource_key,
			int week_number,
			string year_week,
			string quarter)
		{
			var row = dataTable.NewRow();
			row["date_id"] = date_id;
			row["date_date"] = date_date;
			row["year"] = year;
			row["year_month"] = year_month;
			row["month"] = month;
			row["month_name"] = month_name;
			row["month_resource_key"] = month_resource_key;
			row["day_in_month"] = day_in_month;
			row["weekday_number"] = weekday_number;
			row["weekday_name"] = weekday_name;
			row["weekday_resource_key"] = weekday_resource_key;
			row["week_number"] = week_number;
			row["year_week"] = year_week;
			row["quarter"] = quarter;
			row["insert_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}