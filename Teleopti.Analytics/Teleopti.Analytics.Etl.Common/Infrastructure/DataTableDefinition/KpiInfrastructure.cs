using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class KpiInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("kpi_code", typeof(Guid));
			table.Columns.Add("kpi_name", typeof(string));
			table.Columns.Add("resource_key", typeof(string));
			table.Columns.Add("target_value_type", typeof(int));
			table.Columns.Add("default_target_value", typeof(double));
			table.Columns.Add("default_min_value", typeof(double));
			table.Columns.Add("default_max_value", typeof(double));
			table.Columns.Add("default_between_color", typeof(int));
			table.Columns.Add("default_lower_than_min_color", typeof(int));
			table.Columns.Add("default_higher_than_max_color", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
		}
	}
}
