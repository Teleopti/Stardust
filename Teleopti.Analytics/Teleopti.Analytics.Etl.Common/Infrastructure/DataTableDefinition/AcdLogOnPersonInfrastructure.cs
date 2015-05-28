using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class AcdLogOnPersonInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("acd_login_code", typeof(string));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("start_date", typeof(DateTime));
			table.Columns.Add("end_date", typeof(DateTime));
			table.Columns.Add("person_period_code", typeof(Guid));
			table.Columns.Add("log_object_datasource_id", typeof(int));
			table.Columns.Add("log_object_name", typeof(String));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
		}
	}
}

