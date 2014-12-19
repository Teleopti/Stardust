using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_overtime
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_overtime");
			table.Columns.Add("overtime_id", typeof(int));
			table.Columns.Add("overtime_code", typeof(Guid));
			table.Columns.Add("overtime_name");
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
			return table;
		}

		public static void AddOvertime(this DataTable dataTable,
			int overtimeId,
			Guid overtimeCode,
			string overtimeName,
			int businessUnitId,
			int datasourceId,
			bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["overtime_id"] = overtimeId;
			row["overtime_code"] = overtimeCode;
			row["overtime_name"] = overtimeName;
			row["business_unit_id"] = businessUnitId;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["is_deleted"] = toBeDeleted;
			dataTable.Rows.Add(row);
		}
	}
}