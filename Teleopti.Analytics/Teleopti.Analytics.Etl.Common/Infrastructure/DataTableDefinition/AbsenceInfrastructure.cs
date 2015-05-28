using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class AbsenceInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("absence_code", typeof(Guid));
			table.Columns.Add("absence_name", typeof(string));
			table.Columns.Add("absence_shortname", typeof(string));
			table.Columns.Add("display_color", typeof(int));
			table.Columns.Add("display_color_html", typeof(string));
			table.Columns.Add("in_contract_time", typeof(bool));
			table.Columns.Add("in_paid_time", typeof(bool));
			table.Columns.Add("in_work_time", typeof(bool));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(String));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
		}
	}
}
