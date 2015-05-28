using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class DateInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("date_date", typeof(DateTime));
			table.Columns.Add("year", typeof(int));
			table.Columns.Add("year_month", typeof(string));
			table.Columns.Add("month", typeof(int));
			table.Columns.Add("month_name", typeof(string));
			table.Columns.Add("month_resource_key", typeof(string));
			table.Columns.Add("day_in_month", typeof(int));
			table.Columns.Add("weekday_number", typeof(int));
			table.Columns.Add("weekday_name", typeof(string));
			table.Columns.Add("weekday_resource_key", typeof(string));
			table.Columns.Add("week_number", typeof(int));
			table.Columns.Add("year_week", typeof(string));
			table.Columns.Add("quarter", typeof(string));
			table.Columns.Add("insert_date", typeof(DateTime));
		}
	}
}
