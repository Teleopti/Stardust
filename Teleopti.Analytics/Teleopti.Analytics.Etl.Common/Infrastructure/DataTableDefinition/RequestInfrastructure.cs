using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class RequestInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("request_code", typeof(Guid));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("application_datetime", typeof(DateTime));
			table.Columns.Add("request_date", typeof(DateTime));
			table.Columns.Add("request_startdate", typeof(DateTime));
			table.Columns.Add("request_enddate", typeof(DateTime));
			table.Columns.Add("request_type_code", typeof(int));
			table.Columns.Add("request_status_code", typeof(int));
			table.Columns.Add("request_start_date_count", typeof(int));
			table.Columns.Add("request_day_count", typeof(int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
			table.Columns.Add("request_starttime", typeof(DateTime));
			table.Columns.Add("request_endtime", typeof(DateTime));
			table.Columns.Add("absence_code", typeof(Guid));
		}
	}
}
