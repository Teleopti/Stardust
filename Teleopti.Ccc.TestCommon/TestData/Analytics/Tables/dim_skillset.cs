using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_skillset
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_skillset");
			table.Columns.Add("skillset_id", typeof(int));
			table.Columns.Add("skillset_code");
			table.Columns.Add("skillset_name");
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddSkillset(this DataTable dataTable,
			int skillsetId,
			string skillsetCode,
			string skillsetName,
			int businessUnitId,
			int datasourceId)
		{
			var row = dataTable.NewRow();

			row["skillset_id"] = skillsetId;
			row["skillset_code"] = skillsetCode;
			row["skillset_name"] = skillsetName;
			row["business_unit_id"] = businessUnitId;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}