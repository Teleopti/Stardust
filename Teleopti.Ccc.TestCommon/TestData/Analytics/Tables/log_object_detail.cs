using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class log_object_detail
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("dbo.log_object_detail");
			table.Columns.Add("log_object_id", typeof(int));
			table.Columns.Add("detail_id", typeof(int));
			table.Columns.Add("detail_desc", typeof(string));
			table.Columns.Add("proc_name", typeof(string));
			table.Columns.Add("int_value", typeof(int));
			table.Columns.Add("date_value", typeof(DateTime));
			return table;
		}

		public static void AddLogObjectDetail(
			this DataTable dataTable,
			int logObjectId,
			int detailId,
			string detailDesc,
			string procName,
			int intValue,
			DateTime dateValue
		)
		{
			var row = dataTable.NewRow();
			row["log_object_id"] = logObjectId;
			row["detail_id"] = detailId;
			row["detail_desc"] = detailDesc;
			row["proc_name"] = procName;
			row["int_value"] = intValue;
			row["date_value"] = dateValue;
			dataTable.Rows.Add(row);
		}
	}
}