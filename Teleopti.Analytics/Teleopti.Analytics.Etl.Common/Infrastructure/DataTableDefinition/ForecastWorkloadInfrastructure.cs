using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class ForecastWorkloadInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("date", typeof(DateTime));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("start_time", typeof(DateTime));
			table.Columns.Add("workload_code", typeof(Guid));
			table.Columns.Add("scenario_code", typeof(Guid));
			table.Columns.Add("end_time", typeof(DateTime));
			table.Columns.Add("skill_code", typeof(Guid));
			table.Columns.Add("forecasted_calls", typeof(double));  // Including "forecasted_campaign_calls"
			table.Columns.Add("forecasted_emails", typeof(double));
			table.Columns.Add("forecasted_backoffice_tasks", typeof(double));
			table.Columns.Add("forecasted_campaign_calls", typeof(double));
			table.Columns.Add("forecasted_calls_excl_campaign", typeof(double));
			table.Columns.Add("forecasted_talk_time_sec", typeof(double));
			table.Columns.Add("forecasted_campaign_talk_time_s", typeof(double));
			table.Columns.Add("forecasted_talk_time_excl_campaign_s", typeof(double));
			table.Columns.Add("forecasted_after_call_work_s", typeof(double));
			table.Columns.Add("forecasted_campaign_after_call_work_s", typeof(double));
			table.Columns.Add("forecasted_after_call_work_excl_campaign_s", typeof(double));
			table.Columns.Add("forecasted_handling_time_s", typeof(double));
			table.Columns.Add("forecasted_campaign_handling_time_s", typeof(double));
			table.Columns.Add("forecasted_handling_time_excl_campaign_s", typeof(double));
			table.Columns.Add("period_length_min", typeof(double));

			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(String));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
		}
	}
}
