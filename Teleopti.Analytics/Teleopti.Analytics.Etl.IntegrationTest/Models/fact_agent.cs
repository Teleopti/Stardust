using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_agent
    {
        public int date_id { get; set; }
        public short interval_id { get; set; }
        public int acd_login_id { get; set; }
        public int local_date_id { get; set; }
        public short local_interval_id { get; set; }
        public Nullable<decimal> ready_time_s { get; set; }
        public Nullable<decimal> logged_in_time_s { get; set; }
        public Nullable<decimal> not_ready_time_s { get; set; }
        public Nullable<decimal> idle_time_s { get; set; }
        public Nullable<int> direct_outbound_calls { get; set; }
        public Nullable<decimal> direct_outbound_talk_time_s { get; set; }
        public Nullable<int> direct_incoming_calls { get; set; }
        public Nullable<decimal> direct_incoming_calls_talk_time_s { get; set; }
        public Nullable<decimal> admin_time_s { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_acd_login dim_acd_login { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_date dim_date1 { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_interval dim_interval1 { get; set; }
    }
}
