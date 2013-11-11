using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_queue
    {
        public int date_id { get; set; }
        public short interval_id { get; set; }
        public int queue_id { get; set; }
        public int local_date_id { get; set; }
        public short local_interval_id { get; set; }
        public Nullable<decimal> offered_calls { get; set; }
        public Nullable<decimal> answered_calls { get; set; }
        public Nullable<decimal> answered_calls_within_SL { get; set; }
        public Nullable<decimal> abandoned_calls { get; set; }
        public Nullable<decimal> abandoned_calls_within_SL { get; set; }
        public Nullable<decimal> abandoned_short_calls { get; set; }
        public Nullable<decimal> overflow_out_calls { get; set; }
        public Nullable<decimal> overflow_in_calls { get; set; }
        public Nullable<decimal> talk_time_s { get; set; }
        public Nullable<decimal> after_call_work_s { get; set; }
        public Nullable<decimal> handle_time_s { get; set; }
        public Nullable<decimal> speed_of_answer_s { get; set; }
        public Nullable<decimal> time_to_abandon_s { get; set; }
        public Nullable<decimal> longest_delay_in_queue_answered_s { get; set; }
        public Nullable<decimal> longest_delay_in_queue_abandoned_s { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_date dim_date1 { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_interval dim_interval1 { get; set; }
        public virtual dim_queue dim_queue { get; set; }
    }
}
