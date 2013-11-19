using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class v_stg_schedule_load
    {
        public int schedule_date_id { get; set; }
        public int person_id { get; set; }
        public short interval_id { get; set; }
        public System.DateTime activity_starttime { get; set; }
        public Nullable<short> scenario_id { get; set; }
        public Nullable<int> activity_id { get; set; }
        public Nullable<int> absence_id { get; set; }
        public Nullable<int> activity_startdate_id { get; set; }
        public Nullable<int> activity_enddate_id { get; set; }
        public Nullable<System.DateTime> activity_endtime { get; set; }
        public Nullable<int> shift_startdate_id { get; set; }
        public Nullable<System.DateTime> shift_starttime { get; set; }
        public Nullable<int> shift_enddate_id { get; set; }
        public Nullable<System.DateTime> shift_endtime { get; set; }
        public Nullable<int> shift_startinterval_id { get; set; }
        public Nullable<int> shift_category_id { get; set; }
        public Nullable<int> shift_length_id { get; set; }
        public Nullable<int> scheduled_time_m { get; set; }
        public Nullable<int> scheduled_time_absence_m { get; set; }
        public Nullable<int> scheduled_time_activity_m { get; set; }
        public Nullable<int> scheduled_contract_time_m { get; set; }
        public Nullable<int> scheduled_contract_time_activity_m { get; set; }
        public Nullable<int> scheduled_contract_time_absence_m { get; set; }
        public Nullable<int> scheduled_work_time_m { get; set; }
        public Nullable<int> scheduled_work_time_activity_m { get; set; }
        public Nullable<int> scheduled_work_time_absence_m { get; set; }
        public Nullable<int> scheduled_over_time_m { get; set; }
        public Nullable<int> scheduled_ready_time_m { get; set; }
        public Nullable<int> scheduled_paid_time_m { get; set; }
        public Nullable<int> scheduled_paid_time_activity_m { get; set; }
        public Nullable<int> scheduled_paid_time_absence_m { get; set; }
        public Nullable<System.DateTime> last_publish { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public Nullable<int> overtime_id { get; set; }
    }
}
