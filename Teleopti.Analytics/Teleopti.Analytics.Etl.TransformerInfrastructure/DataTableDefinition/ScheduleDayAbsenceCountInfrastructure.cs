using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition
{
    public static class ScheduleDayAbsenceCountInfrastructure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
			table.Columns.Add("schedule_date_local", typeof(DateTime));
            table.Columns.Add("person_code", typeof(Guid));
            table.Columns.Add("scenario_code", typeof(Guid));
            table.Columns.Add("starttime", typeof(DateTime));
            table.Columns.Add("day_absence_code", typeof(Guid));
            table.Columns.Add("day_count", typeof(int));
            table.Columns.Add("business_unit_code", typeof(Guid));
            table.Columns.Add("datasource_id", typeof(int));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
            table.Columns.Add("datasource_update_date", typeof(DateTime));
        }
    }
}
