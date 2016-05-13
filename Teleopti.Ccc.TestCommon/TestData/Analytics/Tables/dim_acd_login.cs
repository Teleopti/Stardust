using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_acd_login
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_acd_login");
			table.Columns.Add("acd_login_id", typeof(int));
			table.Columns.Add("acd_login_agg_id", typeof(int));
			table.Columns.Add("acd_login_original_id");
			table.Columns.Add("acd_login_name");
			table.Columns.Add("log_object_name");
			table.Columns.Add("is_active", typeof(bool));
			table.Columns.Add("time_zone_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddDimAcdLogin(this DataTable dataTable, 
			int acd_login_id, 
			int? acd_login_agg_id, 
			string acd_login_original_id, 
			string acd_login_name, 
			string log_object_name, 
			bool? is_active, 
			int time_zone_id, 
			int datasource_id)
		{
			var row = dataTable.NewRow();
			row["acd_login_id"] = acd_login_id;
			row["acd_login_agg_id"] = acd_login_agg_id ?? (object)DBNull.Value;
			row["acd_login_original_id"] = acd_login_original_id;
			row["acd_login_name"] = acd_login_name;
			row["log_object_name"] = log_object_name;
			row["is_active"] = is_active ?? (object)DBNull.Value;
			row["time_zone_id"] = time_zone_id;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}