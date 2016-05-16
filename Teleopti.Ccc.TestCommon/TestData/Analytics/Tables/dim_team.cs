using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_team
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_team");
			table.Columns.Add("team_id", typeof(int));
			table.Columns.Add("team_code", typeof(Guid));
			table.Columns.Add("team_name");
			table.Columns.Add("scorecard_id", typeof(int));
			table.Columns.Add("site_id", typeof(int));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddTeam(
			this DataTable dataTable,
			int team_id,
			Guid? team_code,
			string team_name,
			int scorecard_id,
			int site_id,
			int business_unit_id,
			int datasource_id)
		{
			var row = dataTable.NewRow();

			row["team_id"] = team_id;
			row["team_code"] = team_code ?? (object)DBNull.Value;
			row["team_name"] = team_name;
			row["scorecard_id"] = scorecard_id;
			row["site_id"] = site_id;
			row["business_unit_id"] = business_unit_id;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.UtcNow;
			row["update_date"] = DateTime.UtcNow;
			row["datasource_update_date"] = DateTime.UtcNow;

			dataTable.Rows.Add(row);
		}
	}
}