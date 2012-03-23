using System;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables
{
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
			int datasource_id)
		{
			var row = dataTable.NewRow();
			row["business_unit_id"] = business_unit_id;
			row["business_unit_code"] = business_unit_code;
			row["business_unit_name"] = business_unit_name;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}