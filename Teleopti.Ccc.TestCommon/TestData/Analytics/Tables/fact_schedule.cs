using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class fact_schedule
	{
		public static DataTable CreateTable()
		{

			var table = new DataTable("mart.fact_schedule");
			table.Columns.Add("shift_start_date_local_Id", typeof(int));
			table.Columns.Add("schedule_date_id", typeof(int));
			table.Columns.Add("person_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("activity_starttime", typeof(DateTime));
			table.Columns.Add("scenario_id", typeof(int));
			table.Columns.Add("activity_id", typeof(int));
			table.Columns.Add("absence_id", typeof(int));
			table.Columns.Add("activity_startdate_id", typeof(int));
			table.Columns.Add("activity_enddate_id", typeof(int));
			table.Columns.Add("activity_endtime", typeof(DateTime));
			table.Columns.Add("shift_startdate_id", typeof(int));
			table.Columns.Add("shift_starttime", typeof(DateTime));
			table.Columns.Add("shift_enddate_id", typeof(int));
			table.Columns.Add("shift_endtime", typeof(DateTime));
			table.Columns.Add("shift_startinterval_id", typeof(int));
			table.Columns.Add("shift_category_id", typeof(int));
			table.Columns.Add("shift_length_id", typeof(int));
			table.Columns.Add("scheduled_time_m", typeof(int));
			table.Columns.Add("scheduled_time_absence_m", typeof(int));
			table.Columns.Add("scheduled_time_activity_m", typeof(int));
			table.Columns.Add("scheduled_contract_time_m", typeof(int));
			table.Columns.Add("scheduled_contract_time_activity_m", typeof(int));
			table.Columns.Add("scheduled_contract_time_absence_m", typeof(int));
			table.Columns.Add("scheduled_work_time_m", typeof(int));
			table.Columns.Add("scheduled_work_time_activity_m", typeof(int));
			table.Columns.Add("scheduled_work_time_absence_m", typeof(int));
			table.Columns.Add("scheduled_over_time_m", typeof(int));
			table.Columns.Add("scheduled_ready_time_m", typeof(int));
			table.Columns.Add("scheduled_paid_time_m", typeof(int));
			table.Columns.Add("scheduled_paid_time_activity_m", typeof(int));
			table.Columns.Add("scheduled_paid_time_absence_m", typeof(int));
			table.Columns.Add("last_publish", typeof(DateTime));
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("overtime_id", typeof(int));
			return table;
		}

		public static void AddFactSchedule(
			this DataTable dataTable,
			int shift_start_date_local_Id,
			int schedule_date_id,
			int person_id,
			int interval_id,
			DateTime activity_starttime,
			int scenario_id,
			int activity_id,
			int absence_id,
			int activity_startdate_id,
			int activity_enddate_id,
			DateTime activity_endtime,
			int shift_startdate_id,
			DateTime shift_starttime,
			int shift_enddate_id,
			DateTime shift_endtime,
			int shift_startinterval_id,
			int shift_category_id,
			int shift_length_id,
			int scheduled_time_m,
			int scheduled_time_absence_m,
			int scheduled_time_activity_m,
			int scheduled_contract_time_m,
			int scheduled_contract_time_activity_m,
			int scheduled_contract_time_absence_m,
			int scheduled_work_time_m,
			int scheduled_work_time_activity_m,
			int scheduled_work_time_absence_m,
			int scheduled_over_time_m,
			int scheduled_ready_time_m,
			int scheduled_paid_time_m,
			int scheduled_paid_time_activity_m,
			int scheduled_paid_time_absence_m,
			int business_unit_id)
		{
			var row = dataTable.NewRow();

			row["shift_start_date_local_Id"] = shift_start_date_local_Id;
			row["schedule_date_id"] = schedule_date_id;
			row["person_id"] = person_id;
			row["interval_id"] = interval_id;
			row["activity_starttime"] = activity_starttime;
			row["scenario_id"] = scenario_id;
			row["activity_id"] = activity_id;
			row["absence_id"] = absence_id;
			row["activity_startdate_id"] = activity_startdate_id;
			row["activity_enddate_id"] = activity_enddate_id;
			row["activity_endtime"] = activity_endtime;
			row["shift_startdate_id"] = shift_startdate_id;
			row["shift_starttime"] = shift_starttime;
			row["shift_enddate_id"] = shift_enddate_id;
			row["shift_endtime"] = shift_endtime;
			row["shift_startinterval_id"] = shift_startinterval_id;
			row["shift_category_id"] = shift_category_id;
			row["shift_length_id"] = shift_length_id;
			row["scheduled_time_m"] = scheduled_time_m;
			row["scheduled_time_absence_m"] = scheduled_time_absence_m;
			row["scheduled_time_activity_m"] = scheduled_time_activity_m;
			row["scheduled_contract_time_m"] = scheduled_contract_time_m;
			row["scheduled_contract_time_activity_m"] = scheduled_contract_time_activity_m;
			row["scheduled_contract_time_absence_m"] = scheduled_contract_time_absence_m;
			row["scheduled_work_time_m"] = scheduled_work_time_m;
			row["scheduled_work_time_activity_m"] = scheduled_work_time_activity_m;
			row["scheduled_work_time_absence_m"] = scheduled_work_time_absence_m;
			row["scheduled_over_time_m"] = scheduled_over_time_m;
			row["scheduled_ready_time_m"] = scheduled_ready_time_m;
			row["scheduled_paid_time_m"] = scheduled_paid_time_m;
			row["scheduled_paid_time_activity_m"] = scheduled_paid_time_activity_m;
			row["scheduled_paid_time_absence_m"] = scheduled_paid_time_absence_m;
			row["last_publish"] = DateTime.Now;
			row["business_unit_id"] = business_unit_id;
			row["datasource_id"] = 10;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["overtime_id"] = 0;
			dataTable.Rows.Add(row);
		}
	}
}