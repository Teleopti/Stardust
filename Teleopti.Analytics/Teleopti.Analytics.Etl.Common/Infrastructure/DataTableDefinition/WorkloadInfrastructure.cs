using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class WorkloadInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("workload_code", typeof(Guid));
			table.Columns.Add("workload_name", typeof(string));
			table.Columns.Add("skill_code", typeof(Guid));
			table.Columns.Add("skill_name", typeof(string));
			table.Columns.Add("time_zone_code", typeof(string));
			table.Columns.Add("forecast_method_code", typeof(Guid));
			table.Columns.Add("forecast_method_name", typeof(string));
			table.Columns.Add("percentage_offered", typeof(double));
			table.Columns.Add("percentage_overflow_in", typeof(double));
			table.Columns.Add("percentage_overflow_out", typeof(double));
			table.Columns.Add("percentage_abandoned", typeof(double));
			table.Columns.Add("percentage_abandoned_short", typeof(double));
			table.Columns.Add("percentage_abandoned_within_service_level", typeof(double));
			table.Columns.Add("percentage_abandoned_after_service_level", typeof(double));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(String));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("skill_is_deleted", typeof(bool));
			table.Columns.Add("is_deleted", typeof(bool));
		}
	}
}
