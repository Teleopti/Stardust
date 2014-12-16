using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_day_off
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_day_off");
			table.Columns.Add("day_off_id", typeof(int));
			table.Columns.Add("day_off_code", typeof(Guid));
			table.Columns.Add("day_off_name");
			table.Columns.Add("display_color", typeof(int));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("display_color_html");
			table.Columns.Add("day_off_shortname");
			return table;
		}

		public static void AddDayOff(this DataTable dataTable,
						int dayOffId,
						Guid dayOffCode,
						string dayOffName,
						int businessUnitId,
						int datasourceId,
						bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["day_off_id"] = dayOffId;
			row["day_off_code"] = dayOffCode;
			row["day_off_name"] = dayOffName;
			row["display_color"] = -8355712;

			row["business_unit_id"] = businessUnitId;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["display_color_html"] = "#808080";
			row["day_off_shortname"] = "";
			dataTable.Rows.Add(row);
		}
	}
}