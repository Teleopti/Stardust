using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class stg_schedule_changed_servicebus
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("stage.stg_schedule_changed_servicebus");
			table.Columns.Add("schedule_date_local", typeof(DateTime));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("scenario_code", typeof(Guid));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddScheduleChange(this DataTable dataTable,
			Guid personCode, DateTime shiftStartLocalDate, Guid scenarioCode, int dataSourceId, Guid businessCode)
		{
			var row = dataTable.NewRow();

			row["schedule_date_local"] = shiftStartLocalDate;
			row["person_code"] = personCode;
			row["scenario_code"] = scenarioCode;
			row["business_unit_code"] = businessCode;
			row["datasource_id"] = dataSourceId;
			row["datasource_update_date"] = DateTime.Now;

			dataTable.Rows.Add(row);
		}

		
	}
}