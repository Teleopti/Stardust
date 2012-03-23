using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables
{
	public static class bridge_queue_workload
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.bridge_queue_workload");
			table.Columns.Add("queue_id");
			table.Columns.Add("workload_id");
			table.Columns.Add("skill_id");
			table.Columns.Add("business_unit_id");
			table.Columns.Add("datasource_id");
			table.Columns.Add("insert_date");
			table.Columns.Add("update_date");
			return table;
		}

		public static void AddRow(
			this DataTable dataTable,
			int queue_id,
			int workload_id,
			int skill_id,
			int business_unit_id,
			int datasource_id)
		{
			var row = dataTable.NewRow();
			row["queue_id"] = queue_id;
			row["workload_id"] = workload_id;
			row["skill_id"] = skill_id;
			row["business_unit_id"] = business_unit_id;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}

	}
}