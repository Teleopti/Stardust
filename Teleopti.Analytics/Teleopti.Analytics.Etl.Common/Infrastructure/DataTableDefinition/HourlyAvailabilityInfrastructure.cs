using System;
using System.Data;

namespace Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition
{
	public static class HourlyAvailabilityInfrastructure
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void AddColumnsToDataTable(DataTable table)
		{
			table.Columns.Add("restriction_date", typeof(DateTime));
			table.Columns.Add("person_code", typeof(Guid));
			table.Columns.Add("scenario_code", typeof(Guid));
			table.Columns.Add("available_time_m", typeof(int));
			table.Columns.Add("scheduled_time_m", typeof(int));
			table.Columns.Add("scheduled", typeof(int));
			table.Columns.Add("business_unit_code", typeof(Guid));
			table.Columns.Add("datasource_id", typeof(int));

		}
	}
}
