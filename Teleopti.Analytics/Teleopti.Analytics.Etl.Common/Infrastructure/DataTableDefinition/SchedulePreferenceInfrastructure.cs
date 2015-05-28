using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class SchedulePreferenceInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("person_restriction_code", typeof(Guid));
			table.Columns.Add("restriction_date", typeof(DateTime));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("scenario_code", typeof(Guid));
			table.Columns.Add("shift_category_code", typeof(Guid));
			table.Columns.Add("day_off_code", typeof(Guid));
			table.Columns.Add("day_off_name", typeof(String));
			table.Columns.Add("day_off_shortname", typeof(String));
			table.Columns.Add("preference_type_id", typeof(int));
			table.Columns.Add("preference_fulfilled", typeof(int));
			table.Columns.Add("preference_unfulfilled", typeof(int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("activity_code", typeof(Guid));
			table.Columns.Add("absence_code", typeof(Guid));
			table.Columns.Add("must_have", typeof(int));
		}
	}
}
