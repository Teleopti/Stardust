using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_activity
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_activity");
			table.Columns.Add("activity_id", typeof(int));
			table.Columns.Add("activity_code", typeof(Guid));
			table.Columns.Add("activity_name");
			table.Columns.Add("display_color", typeof(int));
			table.Columns.Add("in_ready_time", typeof(bool));
			table.Columns.Add("in_ready_time_name");
			table.Columns.Add("in_contract_time", typeof(bool));
			table.Columns.Add("in_contract_time_name");
			table.Columns.Add("in_paid_time", typeof(bool));
			table.Columns.Add("in_paid_time_name");
			table.Columns.Add("in_work_time", typeof(bool));
			table.Columns.Add("in_work_time_name");
			table.Columns.Add("business_unit_id", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
			table.Columns.Add("display_color_html");
			return table;
		}

		public static void AddActivity(this DataTable dataTable,
						int activityId,
						Guid activityCode,
						string activityName,
						int displayColor,
						bool inReadyTime,
						string inReadyTimeName,
						bool inContractTime,
						string inContractTimeName,
						bool inPaidTime,
						string inPaidTimeName,
						bool inWorkTime,
						string inWorkTimeName,
						int businessUnitId,
						int datasourceId,
						bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["activity_id"] = activityId;
			row["activity_code"] = activityCode;
			row["activity_name"] = activityName;
			row["display_color"] = displayColor;

			row["in_ready_time"] = inReadyTime;
			row["in_ready_time_name"] = inReadyTimeName;
			row["in_contract_time"] = inContractTime;
			row["in_contract_time_name"] = inContractTimeName;
			row["in_paid_time"] = inPaidTime;
			row["in_paid_time_name"] = inPaidTimeName;
			row["in_work_time"] = inWorkTime;
			row["in_work_time_name"] = inWorkTimeName;

			row["business_unit_id"] = businessUnitId;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["is_deleted"] = toBeDeleted;
			row["display_color_html"] = "";
			dataTable.Rows.Add(row);
		}

		public static void AddActivity(this DataTable dataTable,
						int activityId,
						Guid activityCode,
						string activityName,
						int displayColor,
						int businessUnitId,
						int datasourceId,
						bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["activity_id"] = activityId;
			row["activity_code"] = activityCode;
			row["activity_name"] = activityName;
			row["display_color"] = displayColor;

			row["in_ready_time"] = true;
			row["in_ready_time_name"] = "InReady";
			row["in_contract_time"] = true;
			row["in_contract_time_name"] = "InContract";
			row["in_paid_time"] = true;
			row["in_paid_time_name"] = "InPaid";
			row["in_work_time"] = true;
			row["in_work_time_name"] = "InWork";

			row["business_unit_id"] = businessUnitId;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["is_deleted"] = toBeDeleted;
			row["display_color_html"] = "";
			dataTable.Rows.Add(row);
		}
	}
}