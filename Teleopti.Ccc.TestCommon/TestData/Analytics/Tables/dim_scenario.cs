using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_scenario
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_scenario");
			table.Columns.Add("scenario_id", typeof(int));
			table.Columns.Add("scenario_code", typeof(Guid));
			table.Columns.Add("scenario_name", typeof(string));
			table.Columns.Add("default_scenario", typeof(bool));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(string));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
			return table;
		}

		public static void AddScenario(this DataTable dataTable,
						int scenarioId,
						Guid scenarioCode,
						string scenarioName,
						bool defaultScenario,
						int businessUnitId,
						Guid businessUnitCode,
						string businessUnitName,
						int datasourceId,
						DateTime insertDate,
						DateTime updateDate,
						DateTime datasourceUpdateDate,
						bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["scenario_id"] = scenarioId;
			row["scenario_code"] = scenarioCode;
			row["scenario_name"] = scenarioName;
			row["default_scenario"] = defaultScenario;
			row["business_unit_id"] = businessUnitId;
			row["business_unit_code"] = businessUnitCode;
			row["business_unit_name"] = businessUnitName;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = insertDate;
			row["update_date"] = updateDate;
			row["datasource_update_date"] = datasourceUpdateDate;
			row["is_deleted"] = toBeDeleted;

			dataTable.Rows.Add(row);
		}
	}
}