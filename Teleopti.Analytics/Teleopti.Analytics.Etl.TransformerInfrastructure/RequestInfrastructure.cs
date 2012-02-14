using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public static class RequestInfrastructure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
            table.Columns.Add("date_from", typeof(DateTime));
            table.Columns.Add("interval_from_id", typeof (int));
            table.Columns.Add("request_code", typeof(Guid));
            table.Columns.Add("person_code", typeof(Guid));
            table.Columns.Add("request_type_code", typeof(int));
            table.Columns.Add("request_status_code", typeof(int));
            table.Columns.Add("date_from_local", typeof(DateTime));
            table.Columns.Add("business_unit_code", typeof(Guid));
            table.Columns.Add("request_day_count", typeof(int));
            table.Columns.Add("request_start_date_count", typeof(int));
            table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			table.Columns.Add("is_deleted", typeof(bool));
        }
    }
}
