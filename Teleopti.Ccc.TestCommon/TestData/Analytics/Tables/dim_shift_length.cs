using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class dim_shift_length
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.dim_shift_length");
			table.Columns.Add("shift_length_id", typeof(int));
			table.Columns.Add("shift_length_m", typeof(int));
			table.Columns.Add("shift_length_h", typeof(double));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			return table;
		}

		
		public static void AddShiftLength(this DataTable dataTable,
			int shiftLengthId,
			int shiftLengthMinutes,
			double shiftLengthHours,
			int datasourceId)
		{
			var row = dataTable.NewRow();

			row["shift_length_id"] = shiftLengthId;
			row["shift_length_m"] = shiftLengthMinutes;
			row["shift_length_h"] = shiftLengthHours;
			row["datasource_id"] = datasourceId;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}