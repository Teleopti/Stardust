using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class fact_forecast_workload
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.fact_forecast_workload");
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("start_time", typeof(DateTime));
			table.Columns.Add("workload_id", typeof(int));
			table.Columns.Add("scenario_id", typeof(int));
			table.Columns.Add("end_time", typeof(DateTime));
			table.Columns.Add("skill_id", typeof(int));
			table.Columns.Add("forecasted_calls", typeof(double));
			table.Columns.Add("forecasted_emails", typeof(double));
			table.Columns.Add("forecasted_backoffice_tasks", typeof(double));
			table.Columns.Add("forecasted_campaign_calls", typeof(double));
			table.Columns.Add("forecasted_calls_excl_campaign", typeof(double));
			table.Columns.Add("forecasted_talk_time_s", typeof(double));
			table.Columns.Add("forecasted_campaign_talk_time_s", typeof(double));
			table.Columns.Add("forecasted_talk_time_excl_campaign_s", typeof(double));
			table.Columns.Add("forecasted_after_call_work_s", typeof(double));
			table.Columns.Add("forecasted_campaign_after_call_work_s", typeof(double));
			table.Columns.Add("forecasted_after_call_work_excl_campaign_s", typeof(double));
			table.Columns.Add("forecasted_handling_time_s", typeof(double));
			table.Columns.Add("forecasted_campaign_handling_time_s", typeof(double));
			table.Columns.Add("forecasted_handling_time_excl_campaign_s", typeof(double));
			table.Columns.Add("period_length_min", typeof(double));
			table.Columns.Add("business_unit_id", typeof(int));
			return table;
		}

		public static void AddForecastWorkload(
			this DataTable dataTable,
			int date_id,
			int interval_id,
			DateTime start_time,
			int workload_id,
			int scenario_id,
			DateTime end_time,
			int skill_id,
			double forecasted_calls,
			double forecasted_emails,
			double forecasted_backoffice_tasks,
			double forecasted_campaign_calls,
			double forecasted_calls_excl_campaign,
			double forecasted_talk_time_s,
			double forecasted_campaign_talk_time_s,
			double forecasted_talk_time_excl_campaign_s,
			double forecasted_after_call_work_s,
			double forecasted_campaign_after_call_work_s,
			double forecasted_after_call_work_excl_campaign_s,
			double forecasted_handling_time_s,
			double forecasted_campaign_handling_time_s,
			double forecasted_handling_time_excl_campaign_s,
			double period_length_min,
			int business_unit_id
			)

		{
			var row = dataTable.NewRow();
			row["date_id"] = date_id;
			row["interval_id"] = interval_id;
			row["start_time"] = start_time;
			row["workload_id"] = workload_id;
			row["scenario_id"] = scenario_id;
			row["end_time"] = end_time;
			row["skill_id"] = skill_id;
			row["forecasted_calls"] = forecasted_calls;
			row["forecasted_emails"] = forecasted_emails;
			row["forecasted_backoffice_tasks"] = forecasted_backoffice_tasks;
			row["forecasted_campaign_calls"] = forecasted_campaign_calls;
			row["forecasted_calls_excl_campaign"] = forecasted_calls_excl_campaign;
			row["forecasted_talk_time_s"] = forecasted_talk_time_s;
			row["forecasted_campaign_talk_time_s"] = forecasted_campaign_talk_time_s;
			row["forecasted_talk_time_excl_campaign_s"] = forecasted_talk_time_excl_campaign_s;
			row["forecasted_after_call_work_s"] = forecasted_after_call_work_s;
			row["forecasted_campaign_after_call_work_s"] = forecasted_campaign_after_call_work_s;
			row["forecasted_after_call_work_excl_campaign_s"] = forecasted_after_call_work_excl_campaign_s;
			row["forecasted_handling_time_s"] = forecasted_handling_time_s;
			row["forecasted_campaign_handling_time_s"] = forecasted_campaign_handling_time_s;
			row["forecasted_handling_time_excl_campaign_s"] = forecasted_handling_time_excl_campaign_s;
			row["period_length_min"] = period_length_min;
			row["business_unit_id"] = business_unit_id;
			dataTable.Rows.Add(row);
		}
	}
}
