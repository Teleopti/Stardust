using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_date
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_date");
			table.Columns.Add("date_id", typeof (int));
			table.Columns.Add("date_date", typeof (DateTime));
			table.Columns.Add("year", typeof (int));
			table.Columns.Add("year_month", typeof (int));
			table.Columns.Add("month", typeof (int));
			table.Columns.Add("month_name");
			table.Columns.Add("month_resource_key");
			table.Columns.Add("day_in_month", typeof (int));
			table.Columns.Add("weekday_number", typeof (int));
			table.Columns.Add("weekday_name");
			table.Columns.Add("weekday_resource_key");
			table.Columns.Add("week_number", typeof (int));
			table.Columns.Add("year_week");
			table.Columns.Add("quarter");
			table.Columns.Add("insert_date");
			return table;
		}

		public static DataRow AddDate(
			this DataTable dataTable,
			int date_id,
			DateTime date_date,
			int year,
			int year_month,
			int month,
			string month_name,
			string month_resource_key,
			int day_in_month,
			DayOfWeek weekday_number,
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
			if (weekday_number == DayOfWeek.Sunday)
				row["weekday_number"] = 7;
			else
				row["weekday_number"] = weekday_number;
			row["weekday_name"] = weekday_name;
			row["weekday_resource_key"] = weekday_resource_key;
			row["week_number"] = week_number;
			row["year_week"] = year_week;
			row["quarter"] = quarter;
			row["insert_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
			return row;
		}

		public static DataRow AddDate(
			this DataTable dataTable,
			int date_id,
			DateTime date_date,
			CultureInfo culture
			)
		{
			return dataTable.AddDate(
				date_id,
				date_date.Date,
				date_date.Year,
				date_date.Year*100 + date_date.Month,
				date_date.Month,
				culture.DateTimeFormat.GetMonthName(date_date.Month),
				date_date.Month.GetMonthResourceKey(),
				date_date.Day,
				date_date.DayOfWeek,
				culture.DateTimeFormat.GetDayName(date_date.DayOfWeek),
				date_date.DayOfWeek.GetWeekDayResourceKey(),
				DateHelper.WeekNumber(date_date, culture),
				(date_date.Year*100 + DateHelper.WeekNumber(date_date, culture)).ToString(culture),
				date_date.Year + "Q" + DateHelper.GetQuarter(date_date.Month)
				);
		}

		public static IEnumerable<int> FindDateIdsByDate(this IEnumerable<DataRow> rows, DateTime date)
		{
			return (
			       	from d in rows.DateRowsByDateQuery(date)
			       	select (int) d["date_id"]
			       )
				.ToArray();
		}

		public static DateTime FindDateByDateId(this IEnumerable<DataRow> rows, int date_id)
		{
			return (
					from d in rows.DateRowsByIdQuery(date_id)
					select (DateTime)d["date_date"]
				   ).Single();
		}

		private static IEnumerable<DataRow> DateRowsByDateQuery(this IEnumerable<DataRow> rows, DateTime date)
		{
			date = date.Date;
			return from d in rows
			       let dd = (DateTime) d["date_date"]
			       where dd == date
			       select d;
		}

		private static IEnumerable<DataRow> DateRowsByIdQuery(this IEnumerable<DataRow> rows, int date_id)
		{
			return from d in rows
				   where (int)d["date_id"] == date_id
				   select d;
		}

	}
}