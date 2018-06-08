using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class acd_type
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("dbo.acd_type");
			table.Columns.Add("acd_type_id", typeof(int));
			table.Columns.Add("acd_type_desc", typeof(string));
			return table;
		}

		public static void AddAcdType(
			this DataTable dataTable,
			int acdTypeId,
			string acdTypeDesc
		)
		{
			var row = dataTable.NewRow();
			row["acd_type_id"] = acdTypeId;
			row["acd_type_desc"] = acdTypeDesc;
			dataTable.Rows.Add(row);
		}
	}
}