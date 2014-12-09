using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_shift_category
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_shift_category");
			table.Columns.Add("shift_category_id", typeof(int));
			table.Columns.Add("shift_category_code", typeof(Guid));
			table.Columns.Add("shift_category_name");
			table.Columns.Add("shift_category_shortname");
			table.Columns.Add("display_color", typeof(int));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
			return table;
		}

		public static void AddShiftCategory(this DataTable dataTable,
						int shiftCategoryId,
						Guid shiftCategoryCode,
						string shiftCategoryName,
						string shiftCategoryShortName,
						int displayColor,
						int businessUnitId,
						int datasourceId,
						bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["shift_category_id"] = shiftCategoryId;
			row["shift_category_code"] = shiftCategoryCode;
			row["shift_category_name"] = shiftCategoryName;
			row["shift_category_shortname"] = shiftCategoryShortName;
			row["display_color"] = displayColor;

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