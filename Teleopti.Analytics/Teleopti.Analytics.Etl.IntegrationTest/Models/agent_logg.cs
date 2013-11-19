using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class agent_logg
    {
        public int queue { get; set; }
        public System.DateTime date_from { get; set; }
        public int interval { get; set; }
        public int agent_id { get; set; }
        public string agent_name { get; set; }
        public Nullable<int> avail_dur { get; set; }
        public Nullable<int> tot_work_dur { get; set; }
        public Nullable<int> talking_call_dur { get; set; }
        public Nullable<int> pause_dur { get; set; }
        public Nullable<int> wait_dur { get; set; }
        public Nullable<int> wrap_up_dur { get; set; }
        public Nullable<int> answ_call_cnt { get; set; }
        public Nullable<int> direct_out_call_cnt { get; set; }
        public Nullable<int> direct_out_call_dur { get; set; }
        public Nullable<int> direct_in_call_cnt { get; set; }
        public Nullable<int> direct_in_call_dur { get; set; }
        public Nullable<int> transfer_out_call_cnt { get; set; }
        public Nullable<int> admin_dur { get; set; }
        public virtual agent_info agent_info { get; set; }
        public virtual queue queue1 { get; set; }
    }
}
