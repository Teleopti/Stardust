using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class IntervalInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("interval_name", typeof(string));
			table.Columns.Add("halfhour_name", typeof(string));
			table.Columns.Add("hour_name", typeof(string));
			table.Columns.Add("interval_start", typeof(DateTime));
			table.Columns.Add("interval_end", typeof(DateTime));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
		}
	}
}
