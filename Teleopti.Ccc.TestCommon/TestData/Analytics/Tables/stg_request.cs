using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class stg_request
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("stage.stg_request");
			table.Columns.Add("request_code", typeof(Guid));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("application_datetime", typeof (DateTime));
			table.Columns.Add("request_date", typeof (DateTime));
			table.Columns.Add("request_startdate", typeof (DateTime));
			table.Columns.Add("request_enddate", typeof (DateTime));
			table.Columns.Add("request_type_code", typeof(int));
			table.Columns.Add("request_status_code", typeof (int));
			table.Columns.Add("request_start_date_count", typeof (int));
			table.Columns.Add("request_day_count", typeof (int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("datasource_id", typeof (int));
			table.Columns.Add("insert_date", typeof (DateTime));
			table.Columns.Add("update_date", typeof (DateTime));
			table.Columns.Add("datasource_update_date", typeof (DateTime));
			table.Columns.Add("is_deleted", typeof (int));
			table.Columns.Add("request_starttime", typeof (DateTime));
			table.Columns.Add("request_endtime", typeof (DateTime));
			table.Columns.Add("absence_code", typeof(Guid));

			return table;
		}

		// add more later if we need other setup
		public static void AddStageRequest(
			this DataTable dataTable,
			Guid requestCode,
			Guid personCode,
			DateTime requestDate,
			int requestTypeCode,
			int requestStatusCode,
			Guid buCode,
			Guid absenceCode,
			int datasourceId )
		{
			var row = dataTable.NewRow();
			row["request_code"] = requestCode;
			row["person_code"] = personCode;
			row["application_datetime"] = DateTime.Now;
			row["request_date"] = requestDate;
			row["request_startdate"] = requestDate;
			row["request_enddate"] = requestDate;
			row["request_type_code"] = requestTypeCode;
			row["request_status_code"] = requestStatusCode;
			row["request_start_date_count"] = 1;
			row["request_day_count"] = 1;
			row["business_unit_code"] = buCode;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			row["is_deleted"] = 0;
			row["request_starttime"] = DateTime.Now;
			row["request_endtime"] = DateTime.Now;
			row["absence_code"] = absenceCode;
			dataTable.Rows.Add(row);
		}
	}
}
