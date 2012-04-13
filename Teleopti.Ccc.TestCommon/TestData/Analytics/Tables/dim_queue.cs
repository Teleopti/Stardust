using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_queue
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_queue");
			table.Columns.Add("queue_id", typeof(int));
			table.Columns.Add("queue_agg_id", typeof(int));
			table.Columns.Add("queue_original_id");
			table.Columns.Add("queue_name");
			table.Columns.Add("queue_description");
			table.Columns.Add("log_object_name");
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddQueue(
			this DataTable dataTable,
			int queue_id,
			int queue_agg_id,
			string queue_original_id,
			string queue_name,
			string queue_description,
			string log_object_name,
			int datasource_id)
		{
			var row = dataTable.NewRow();
			row["queue_id"] = queue_id;
			row["queue_agg_id"] = queue_agg_id;
			row["queue_original_id"] = queue_original_id;
			row["queue_name"] = queue_name;
			row["queue_description"] = queue_description;
			row["log_object_name"] = log_object_name;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}