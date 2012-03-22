using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Model
{
	public static class dim_skill
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_skill");
			table.Columns.Add("skill_id", typeof (int));
			table.Columns.Add("skill_code", typeof (Guid));
			table.Columns.Add("skill_name");
			table.Columns.Add("time_zone_id", typeof (int));
			table.Columns.Add("forecast_method_code", typeof (Guid));
			table.Columns.Add("forecast_method_name");
			table.Columns.Add("business_unit_id", typeof (int));
			table.Columns.Add("datasource_id", typeof (int));
			table.Columns.Add("insert_date", typeof (DateTime));
			table.Columns.Add("update_date", typeof (DateTime));
			table.Columns.Add("datasource_update_date", typeof (DateTime));
			table.Columns.Add("is_deleted", typeof (bool));
			return table;
		}

		public static void AddRow(
			this DataTable dataTable,
			int skill_id,
			Guid skill_code,
			string skill_name,
			int time_zone_id,
			Guid forecast_method_code,
			string forecast_method_name,
			int business_unit_id,
			int datasource_id,
			DateTime insert_date,
			DateTime update_date,
			DateTime datasource_update_date,
			bool is_deleted)
		{
			var row = dataTable.NewRow();
			row["skill_id"] = skill_id;
			row["skill_code"] = skill_code;
			row["skill_name"] = skill_name;
			row["time_zone_id"] = time_zone_id;
			row["forecast_method_code"] = forecast_method_code;
			row["forecast_method_name"] = forecast_method_name;
			row["business_unit_id"] = business_unit_id;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = insert_date;
			row["update_date"] = update_date;
			row["datasource_update_date"] = datasource_update_date;
			row["is_deleted"] = is_deleted;
			dataTable.Rows.Add(row);
		}
	}
}