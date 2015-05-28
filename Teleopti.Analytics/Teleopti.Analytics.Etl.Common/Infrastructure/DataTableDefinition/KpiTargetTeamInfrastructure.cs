using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class KpiTargetTeamInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("kpi_code", typeof(Guid));
			table.Columns.Add("team_code", typeof(Guid));
			table.Columns.Add("target_value", typeof(double));
			table.Columns.Add("min_value", typeof(double));
			table.Columns.Add("max_value", typeof(double));
			table.Columns.Add("between_color", typeof(int));
			table.Columns.Add("lower_than_min_color", typeof(int));
			table.Columns.Add("higher_than_max_color", typeof(int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(String));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
		}
	}
}
