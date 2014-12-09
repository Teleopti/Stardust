using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_absence
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_absence");
			table.Columns.Add("absence_id", typeof(int));
			table.Columns.Add("absence_code", typeof(Guid));
			table.Columns.Add("absence_name");
			table.Columns.Add("display_color", typeof(int));
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
			table.Columns.Add("absence_shortname");
			return table;
		}

		public static void AddAbsence(this DataTable dataTable,
						int absenceId,
						Guid absenceCode,
						string absenceName,
						int displayColor,
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

			row["absence_id"] = absenceId;
			row["absence_code"] = absenceCode;
			row["absence_name"] = absenceName;
			row["display_color"] = displayColor;

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
			row["absence_shortname"] = "";
			dataTable.Rows.Add(row);
		}

		public static void AddAbsence(this DataTable dataTable,
						int absenceId,
						Guid absenceCode,
						string absenceName,
						int displayColor,
						int businessUnitId,
						int datasourceId,
						bool toBeDeleted)
		{
			var row = dataTable.NewRow();

			row["absence_id"] = absenceId;
			row["absence_code"] = absenceCode;
			row["absence_name"] = absenceName;
			row["display_color"] = displayColor;

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
			row["absence_shortname"] = "";
			dataTable.Rows.Add(row);
		}
	}
}