using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class PersonInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("valid_from_date", typeof(DateTime));
			table.Columns.Add("valid_to_date", typeof(DateTime));
			table.Columns.Add("valid_from_interval_id", typeof(int));
			table.Columns.Add("valid_to_interval_id", typeof(int));
			table.Columns.Add("valid_to_interval_start", typeof(DateTime));
			table.Columns.Add("person_period_code", typeof(Guid));
			table.Columns.Add("person_name", typeof(String));
			table.Columns.Add("person_first_name", typeof(String));
			table.Columns.Add("person_last_name", typeof(String));
			table.Columns.Add("team_code", typeof(Guid));
			table.Columns.Add("team_name", typeof(String));
			table.Columns.Add("scorecard_code", typeof(Guid));
			table.Columns.Add("site_code", typeof(Guid));
			table.Columns.Add("site_name", typeof(String));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("business_unit_name", typeof(String));
			table.Columns.Add("email", typeof(String));
			table.Columns.Add("note", typeof(String));
			table.Columns.Add("employment_number", typeof(String));
			table.Columns.Add("employment_start_date", typeof(DateTime));
			table.Columns.Add("employment_end_date", typeof(DateTime));
			table.Columns.Add("time_zone_id", typeof(String));
			table.Columns.Add("is_agent", typeof(bool));
			table.Columns.Add("is_user", typeof(bool));
			table.Columns.Add("contract_code", typeof(Guid));
			table.Columns.Add("contract_name", typeof(String));
			table.Columns.Add("parttime_code", typeof(Guid));
			table.Columns.Add("parttime_percentage", typeof(String));
			table.Columns.Add("employment_type", typeof(String));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("windows_domain", typeof(String));
			table.Columns.Add("windows_username", typeof(String));
			table.Columns.Add("valid_from_date_local", typeof(DateTime));
			table.Columns.Add("valid_to_date_local", typeof(DateTime));
		}
	}
}
