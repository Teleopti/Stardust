using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public static class PmUserInfrastructure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
            table.Columns.Add("user_name", typeof(string));
            table.Columns.Add("is_windows_logon", typeof(bool));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
        }
    }
}
