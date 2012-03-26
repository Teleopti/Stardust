using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables
{
	public static class dim_workload
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_workload");
			table.Columns.Add("workload_id", typeof(int));
			table.Columns.Add("workload_code", typeof(Guid));
			table.Columns.Add("workload_name");
			table.Columns.Add("skill_id", typeof(int));
			table.Columns.Add("skill_code", typeof(Guid));
			table.Columns.Add("skill_name");
			table.Columns.Add("time_zone_id", typeof(int));
			table.Columns.Add("forecast_method_code", typeof(Guid));
			table.Columns.Add("forecast_method_name");
			table.Columns.Add("percentage_offered", typeof(float));
			table.Columns.Add("percentage_overflow_in", typeof(float));
			table.Columns.Add("percentage_overflow_out", typeof(float));
			table.Columns.Add("percentage_abandoned", typeof(float));
			table.Columns.Add("percentage_abandoned_short", typeof(float));
			table.Columns.Add("percentage_abandoned_within_service_level", typeof(float));
			table.Columns.Add("percentage_abandoned_after_service_level", typeof(float));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
			return table;
		}

		public static void AddWorkload(
			this DataTable dataTable,
			int workload_id,
			Guid workload_code,
			string workload_name,
			int skill_id,
			Guid skill_code,
			string skill_name,
			int time_zone_id,
			Guid forecast_method_code,
			string forecast_method_name,
			float percentage_offered,
			float percentage_overflow_in,
			float percentage_overflow_out,
			float percentage_abandoned,
			float percentage_abandoned_short,
			float percentage_abandoned_within_service_level,
			float percentage_abandoned_after_service_level,
			int business_unit_id,
			int datasource_id)
		{
			var row = dataTable.NewRow();
			row["workload_id"] = workload_id;
			row["workload_code"] = workload_code;
			row["workload_name"] = workload_name;
			row["skill_id"] = skill_id;
			row["skill_code"] = skill_code;
			row["skill_name"] = skill_name;
			row["time_zone_id"] = time_zone_id;
			row["forecast_method_code"] = forecast_method_code;
			row["forecast_method_name"] = forecast_method_name;
			row["percentage_offered"] = percentage_offered;
			row["percentage_overflow_in"] = percentage_overflow_in;
			row["percentage_overflow_out"] = percentage_overflow_out;
			row["percentage_abandoned"] = percentage_abandoned;
			row["percentage_abandoned_short"] = percentage_abandoned_short;
			row["percentage_abandoned_within_service_level"] = percentage_abandoned_within_service_level;
			row["percentage_abandoned_after_service_level"] = percentage_abandoned_after_service_level;
			row["business_unit_id"] = business_unit_id;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["is_deleted"] = false;
			dataTable.Rows.Add(row);
		}
	}
}