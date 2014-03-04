using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_schedule
    {
		public System.DateTime schedule_date_local { get; set; }
		public System.DateTime schedule_date_utc { get; set; }
        public System.Guid person_code { get; set; }
        public int interval_id { get; set; }
        public System.DateTime activity_start { get; set; }
        public System.Guid scenario_code { get; set; }
        public Nullable<System.Guid> activity_code { get; set; }
        public Nullable<System.Guid> absence_code { get; set; }
        public System.DateTime activity_end { get; set; }
        public System.DateTime shift_start { get; set; }
        public System.DateTime shift_end { get; set; }
        public int shift_startinterval_id { get; set; }
        public Nullable<System.Guid> shift_category_code { get; set; }
        public int shift_length_m { get; set; }
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
        public System.DateTime last_publish { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public Nullable<System.Guid> overtime_code { get; set; }
    }
}
