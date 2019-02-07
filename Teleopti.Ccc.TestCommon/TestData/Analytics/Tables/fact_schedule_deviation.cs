using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class fact_schedule_deviation
	{
		public static DataTable CreateTable()
		{

			var table = new DataTable("mart.fact_schedule_deviation");
			table.Columns.Add("shift_startdate_local_id", typeof(int));
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("person_id", typeof(int));
			table.Columns.Add("scheduled_ready_time_s", typeof(int));
			table.Columns.Add("ready_time_s", typeof(int));
			table.Columns.Add("contract_time_s", typeof(int));
			table.Columns.Add("deviation_schedule_s", typeof(int));
			table.Columns.Add("deviation_schedule_ready_s", typeof(int));
			table.Columns.Add("deviation_contract_s", typeof(int));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof (DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("is_logged_in", typeof(bool));
			table.Columns.Add("shift_startdate_id", typeof (int));
			table.Columns.Add("shift_startinterval_id", typeof (int));
			return table;
		}

		public static void AddFactScheduleDeviation(this DataTable dataTable,
			int shiftStartdateLocalId,
			int shiftStartDateId,
			int dateId,
			int intervalId,
			int personId,
			int contractTimeS,
			int deviationScheduleS,
			int deviationScheduleReadyS,
			int deviationContractS,
			bool isLoggedIn,
			int? businessunitId)
		{
			var row = dataTable.NewRow();
			row["shift_startdate_local_id"] = shiftStartdateLocalId;
			row["date_id"] = dateId;
			row["interval_id"] = intervalId;
			row["person_id"] = personId;
			row["contract_time_s"] = contractTimeS;
			row["deviation_schedule_s"] = deviationScheduleS;
			row["deviation_schedule_ready_s"] = deviationScheduleReadyS;
			row["deviation_contract_s"] = deviationContractS;
			row["is_logged_in"] = isLoggedIn;
			row["shift_startdate_id"] = shiftStartDateId;
			row["shift_startinterval_id"] = intervalId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			if (businessunitId.HasValue)
			{
				row["business_unit_id"] = businessunitId.Value;
			}
			dataTable.Rows.Add(row);
		}
	}
}