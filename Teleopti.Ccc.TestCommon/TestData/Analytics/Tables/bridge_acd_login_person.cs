using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class bridge_acd_login_person
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.bridge_acd_login_person");
			table.Columns.Add("acd_login_id");
			table.Columns.Add("person_id");
			table.Columns.Add("team_id");
			table.Columns.Add("business_unit_id");
			table.Columns.Add("datasource_id");
			table.Columns.Add("insert_date");
			table.Columns.Add("update_date");
			table.Columns.Add("datasource_update_date");
			return table;
		}

		public static void AddAcdLogin(
						this DataTable dataTable,
						int acdLoginId,
						int personId,
						int teamId,
						int businessUnitId,
						int datasourceId)
		{
			var dummyDate = DateTime.Now;
			var row = dataTable.NewRow();
			row["acd_login_id"] = acdLoginId;
			row["person_id"] = personId;
			row["team_id"] = teamId;
			row["business_unit_id"] = businessUnitId;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = dummyDate;
			row["update_date"] = dummyDate;
			row["datasource_update_date"] = dummyDate;
			dataTable.Rows.Add(row);
		}
	}
}