using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_queue
    {
        public System.DateTime date { get; set; }
        public string interval { get; set; }
        public Nullable<int> queue_code { get; set; }
        public string queue_name { get; set; }
        public Nullable<int> offd_direct_call_cnt { get; set; }
        public Nullable<int> overflow_in_call_cnt { get; set; }
        public Nullable<int> aband_call_cnt { get; set; }
        public Nullable<int> overflow_out_call_cnt { get; set; }
        public Nullable<int> answ_call_cnt { get; set; }
        public Nullable<int> queued_and_answ_call_dur { get; set; }
        public Nullable<int> queued_and_aband_call_dur { get; set; }
        public Nullable<int> talking_call_dur { get; set; }
        public Nullable<int> wrap_up_dur { get; set; }
        public Nullable<int> queued_answ_longest_que_dur { get; set; }
        public Nullable<int> queued_aband_longest_que_dur { get; set; }
        public Nullable<int> avg_avail_member_cnt { get; set; }
        public Nullable<int> ans_servicelevel_cnt { get; set; }
        public Nullable<int> wait_dur { get; set; }
        public Nullable<int> aband_short_call_cnt { get; set; }
        public Nullable<int> aband_within_sl_cnt { get; set; }
    }
}
