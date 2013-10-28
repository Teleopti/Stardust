using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_request_status
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_request_status");
			table.Columns.Add("request_status_id", typeof (int));
			table.Columns.Add("request_status_name");
			table.Columns.Add("resource_key");
			return table;
		}

		public static void AddRequestStatus(
			this DataTable dataTable,
			int request_status_id,
			string request_status_name,
			string resouce_key)
		{
			var row = dataTable.NewRow();
			row["request_status_id"] = request_status_id;
			row["request_status_name"] = request_status_name;
			row["resource_key"] = resouce_key;
			dataTable.Rows.Add(row);
		}
	}
}
