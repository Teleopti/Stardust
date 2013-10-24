using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_agent_queue
    {
        public int date_id { get; set; }
        public short interval_id { get; set; }
        public int queue_id { get; set; }
        public int acd_login_id { get; set; }
        public int local_date_id { get; set; }
        public short local_interval_id { get; set; }
        public Nullable<decimal> talk_time_s { get; set; }
        public Nullable<decimal> after_call_work_time_s { get; set; }
        public Nullable<int> answered_calls { get; set; }
        public Nullable<int> transfered_calls { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_acd_login dim_acd_login { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_date dim_date1 { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_interval dim_interval1 { get; set; }
        public virtual dim_queue dim_queue { get; set; }
    }
}
