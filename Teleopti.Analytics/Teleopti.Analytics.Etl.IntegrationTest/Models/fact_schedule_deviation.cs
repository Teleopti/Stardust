using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_schedule_deviation
    {
        public int date_id { get; set; }
        public short interval_id { get; set; }
        public int person_id { get; set; }
        public Nullable<int> scheduled_ready_time_s { get; set; }
        public Nullable<int> ready_time_s { get; set; }
        public Nullable<int> contract_time_s { get; set; }
        public Nullable<decimal> deviation_schedule_s { get; set; }
        public Nullable<decimal> deviation_schedule_ready_s { get; set; }
        public Nullable<decimal> deviation_contract_s { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public bool is_logged_in { get; set; }
        public Nullable<int> shift_startdate_id { get; set; }
        public Nullable<short> shift_startinterval_id { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_person dim_person { get; set; }
    }
}
