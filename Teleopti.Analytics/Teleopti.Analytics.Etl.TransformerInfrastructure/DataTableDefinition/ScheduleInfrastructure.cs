using System;
using System.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition
{
    public static class ScheduleInfrastructure
    {
        //public static DataTable[] CreateEmptyDataTable(int buckets)
        //{
        //    DataTable[] dt = new DataTable[buckets];

        //    for (int i = 0; i < buckets; i++)
        //    {
        //        dt[i] = CreateEmptyDataTable();
        //    }


        //    return new DataTable[buckets];
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void AddColumnsToDataTable(DataTable table)
        {
			table.Columns.Add("schedule_date_local", typeof(DateTime)); 
			table.Columns.Add("schedule_date_utc", typeof(DateTime));
            table.Columns.Add("person_code", typeof(Guid));
            table.Columns.Add("interval_id", typeof(int));
            table.Columns.Add("activity_start", typeof(DateTime));
            table.Columns.Add("scenario_code", typeof(Guid));
            table.Columns.Add("activity_code", typeof(Guid));
            table.Columns.Add("absence_code", typeof(Guid));
            table.Columns.Add("activity_end", typeof(DateTime));
            table.Columns.Add("shift_start", typeof(DateTime));
            table.Columns.Add("shift_end", typeof(DateTime));
            table.Columns.Add("shift_startinterval_id", typeof(int));
			table.Columns.Add("shift_endinterval_id", typeof(int));
            table.Columns.Add("shift_category_code", typeof(Guid));
            table.Columns.Add("shift_length_m", typeof(int));
            table.Columns.Add("scheduled_time_m", typeof(int));
            table.Columns.Add("scheduled_time_absence_m", typeof(int));
            table.Columns.Add("scheduled_time_activity_m", typeof(int));
            table.Columns.Add("scheduled_contract_time_m", typeof(int));
            table.Columns.Add("scheduled_contract_time_activity_m", typeof(int));
            table.Columns.Add("scheduled_contract_time_absence_m", typeof(int));
            table.Columns.Add("scheduled_work_time_m", typeof(int));
            table.Columns.Add("scheduled_work_time_activity_m", typeof(int));
            table.Columns.Add("scheduled_work_time_absence_m", typeof(int));
            table.Columns.Add("scheduled_over_time_m", typeof(int));
            table.Columns.Add("scheduled_ready_time_m", typeof(int));
            table.Columns.Add("scheduled_paid_time_m", typeof(int));
            table.Columns.Add("scheduled_paid_time_activity_m", typeof(int));
            table.Columns.Add("scheduled_paid_time_absence_m", typeof(int));
            table.Columns.Add("last_publish", typeof(DateTime));
            table.Columns.Add("business_unit_code", typeof(Guid));
            table.Columns.Add("business_unit_name", typeof(String));
            table.Columns.Add("datasource_id", typeof(int));
            table.Columns.Add("insert_date", typeof(DateTime));
            table.Columns.Add("update_date", typeof(DateTime));
            table.Columns.Add("datasource_update_date", typeof(DateTime));
        	table.Columns.Add("overtime_code", typeof (Guid));
        }
    }
}
