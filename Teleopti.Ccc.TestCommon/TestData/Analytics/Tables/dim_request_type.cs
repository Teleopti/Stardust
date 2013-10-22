using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_request_type
	{
		public static DataTable CreatTable()
		{
			var table = new DataTable("mart.dim_request_type");
			table.Columns.Add("request_type_id", typeof(int));
			table.Columns.Add("request_type_name");
			table.Columns.Add("resource_key");
			return table;
		}

		public static void AddRequestType(
			this DataTable dataTable,
			int request_type_id,
			string request_type_name,
			string resource_key)
		{
			var row = dataTable.NewRow();
			row["request_type_id"] = request_type_id;
			row["request_type_name"] = request_type_name;
			row["resource_key"] = resource_key;
			dataTable.Rows.Add(row);
		}
	}
}
