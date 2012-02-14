using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public static class ScheduleForecastSkillInfrastructure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
            table.Columns.Add("date", typeof(DateTime));
            table.Columns.Add("interval_id", typeof(int));
            table.Columns.Add("skill_code", typeof(Guid));
            table.Columns.Add("scenario_code", typeof(Guid));

            table.Columns.Add("forecasted_resources_m", typeof(double));
            table.Columns.Add("forecasted_resources", typeof(double));
            table.Columns.Add("forecasted_resources_incl_shrinkage_m", typeof(double));
            table.Columns.Add("forecasted_resources_incl_shrinkage", typeof(double));
            table.Columns.Add("scheduled_resources_m", typeof(double));
            table.Columns.Add("scheduled_resources", typeof(double));
            table.Columns.Add("scheduled_resources_incl_shrinkage_m", typeof(double));
            table.Columns.Add("scheduled_resources_incl_shrinkage", typeof(double));

            table.Columns.Add("business_unit_code", typeof(Guid));
            table.Columns.Add("business_unit_name", typeof(String));
            table.Columns.Add("datasource_id", typeof(int));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
        }
    }
}
