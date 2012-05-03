﻿using System;
using System.Data;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition
{
    public static class SchedulePreferenceInfrastructure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
            table.Columns.Add("person_restriction_code", typeof(Guid));
            table.Columns.Add("restriction_date", typeof(DateTime));
            table.Columns.Add("person_code", typeof(Guid));
            table.Columns.Add("interval_id", typeof(int));
            table.Columns.Add("scenario_code", typeof(Guid));
            table.Columns.Add("shift_category_code", typeof(Guid));
            table.Columns.Add("day_off_code", typeof(Guid));
            table.Columns.Add("day_off_name", typeof(String));
            table.Columns.Add("StartTimeMinimum", typeof(long));
            table.Columns.Add("StartTimeMaximum", typeof(long));
            table.Columns.Add("endTimeMinimum", typeof(long));
            table.Columns.Add("endTimeMaximum", typeof(long));
            table.Columns.Add("WorkTimeMinimum", typeof(long));
            table.Columns.Add("WorkTimeMaximum", typeof(long));
            table.Columns.Add("preference_accepted", typeof(int));
            table.Columns.Add("preference_declined", typeof(int));
            table.Columns.Add("business_unit_code", typeof(Guid));
            table.Columns.Add("datasource_id", typeof(int));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
            table.Columns.Add("datasource_update_date", typeof(DateTime));
        }
    }
}
