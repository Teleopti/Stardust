using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class log_object
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("dbo.log_object");
			table.Columns.Add("log_object_id", typeof(int));
			table.Columns.Add("acd_type_id", typeof(int));
			table.Columns.Add("log_object_desc", typeof(string));
			table.Columns.Add("logDB_name", typeof(string));
			table.Columns.Add("intervals_per_day", typeof(int));
			table.Columns.Add("default_service_level_sec", typeof(int));
			table.Columns.Add("default_short_call_treshold", typeof(int));
			return table;
		}

		public static void AddLogObject(
			this DataTable dataTable,
			int logObjectId,
			int acdTypeId,
			string logObjectDesc,
			string logDbName,
			int intervalsperDay,
			int defaultServiceLevelSec,
			int defaultShortCallTreshold
		)
		{
			var row = dataTable.NewRow();
			row["log_object_id"] = logObjectId;
			row["acd_type_id"] = acdTypeId;
			row["log_object_desc"] = logObjectDesc;
			row["logDB_name"] = logDbName;
			row["intervals_per_day"] = intervalsperDay;
			row["default_service_level_sec"] = defaultServiceLevelSec;
			row["default_short_call_treshold"] = defaultShortCallTreshold;
			dataTable.Rows.Add(row);
		}
	}
}