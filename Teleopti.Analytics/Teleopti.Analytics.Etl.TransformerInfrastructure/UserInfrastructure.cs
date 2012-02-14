using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public static class UserInfrastructure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
            table.Columns.Add("person_code", typeof(Guid));
            table.Columns.Add("person_first_name", typeof(String));
            table.Columns.Add("person_last_name", typeof(String));
            table.Columns.Add("application_logon_name", typeof(String));
            table.Columns.Add("windows_logon_name", typeof(String));
            table.Columns.Add("windows_domain_name", typeof(String));
            table.Columns.Add("password", typeof(String));
            table.Columns.Add("email", typeof(String));
            table.Columns.Add("language_id", typeof(int));
            table.Columns.Add("language_name", typeof(String));
            table.Columns.Add("culture", typeof(int));
            table.Columns.Add("datasource_id", typeof(int));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
            table.Columns.Add("datasource_update_date", typeof(DateTime));
        }
    }
}
