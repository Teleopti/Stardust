using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class TimeZoneInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDimensionDataTable(DataTable table)
		{
			table.Columns.Add("time_zone_code", typeof(String));
			table.Columns.Add("time_zone_name", typeof(String));
			table.Columns.Add("default_zone", typeof(bool));
			table.Columns.Add("utc_conversion", typeof(int));
			table.Columns.Add("utc_conversion_dst", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("utc_in_use", typeof(bool));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToBridgeDataTable(DataTable table)
		{
			table.Columns.Add("date", typeof(DateTime));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("time_zone_code", typeof(String));
			table.Columns.Add("local_date", typeof(DateTime));
			table.Columns.Add("local_interval_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
		}
	}
}
