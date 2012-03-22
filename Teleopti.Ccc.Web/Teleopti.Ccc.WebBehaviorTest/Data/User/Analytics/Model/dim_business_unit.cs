using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Model
{
	public static class dim_queue
	{
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

		public static void AddRow(
			this DataTable dataTable,
			int queue_id,
			int queue_agg_id,
			string queue_original_id,
			string business_unit_name,
			int datasource_id,
			DateTime insert_date,
			DateTime update_date,
			DateTime datasource_update_date)
		{
			var row = dataTable.NewRow();
			row["business_unit_id"] = business_unit_id;
			row["business_unit_code"] = business_unit_code;
			row["business_unit_name"] = business_unit_name;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = insert_date;
			row["update_date"] = update_date;
			row["datasource_update_date"] = datasource_update_date;
			dataTable.Rows.Add(row);
		}
	}

	public static class dim_business_unit
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_business_unit");
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name");
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddRow(
			this DataTable dataTable,
			int business_unit_id,
			Guid business_unit_code,
			string business_unit_name,
			int datasource_id,
			DateTime insert_date,
			DateTime update_date,
			DateTime datasource_update_date)
		{
			var row = dataTable.NewRow();
			row["business_unit_id"] = business_unit_id;
			row["business_unit_code"] = business_unit_code;
			row["business_unit_name"] = business_unit_name;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = insert_date;
			row["update_date"] = update_date;
			row["datasource_update_date"] = datasource_update_date;
			dataTable.Rows.Add(row);
		}
	}
}