using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition
{
    public static class StateGroupInfrastructure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
            table.Columns.Add("state_group_code", typeof(Guid));
            table.Columns.Add("state_group_name", typeof(string));
            table.Columns.Add("business_unit_code", typeof(Guid));
            table.Columns.Add("datasource_id", typeof(int));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
            table.Columns.Add("datasource_update_date", typeof(DateTime));
            table.Columns.Add("is_deleted", typeof(bool));
        }
    }
}