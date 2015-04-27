using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class sys_configuration
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.sys_configuration");
			table.Columns.Add("key", typeof(string));
			table.Columns.Add("value", typeof(string));
			table.Columns.Add("insert_date", typeof(DateTime));
			return table;
		}

		public static void AddConfiguration(this DataTable dataTable,
			string key,
			string value)
		{
			var row = dataTable.NewRow();

			row["key"] = key;
			row["value"] = value;
			row["insert_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}