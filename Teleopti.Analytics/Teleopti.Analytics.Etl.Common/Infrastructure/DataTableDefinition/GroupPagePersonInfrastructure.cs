using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class GroupPagePersonInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("group_page_code", typeof(Guid));
			table.Columns.Add("group_page_name", typeof(string));
			table.Columns.Add("group_page_name_resource_key", typeof(string));
			table.Columns.Add("group_code", typeof(Guid));
			table.Columns.Add("group_name", typeof(string));
			table.Columns.Add("group_is_custom", typeof(bool));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(String));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
		}
	}
}
