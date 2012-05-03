using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition
{
	public static class OvertimeInfrastructure
	{
		public static void AddColumnsToTable(DataTable table)
		{
			table.Columns.Add("overtime_code", typeof(Guid));
			table.Columns.Add("overtime_name", typeof(string));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(string));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
		}
	}
}
