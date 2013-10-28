using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class log_object
    {
        public log_object()
        {
            this.agent_info = new List<agent_info>();
            this.quality_info = new List<quality_info>();
            this.log_object_add_hours = new List<log_object_add_hours>();
            this.log_object_detail = new List<log_object_detail>();
            this.queues = new List<queue>();
        }

        public int log_object_id { get; set; }
        public int acd_type_id { get; set; }
        public string log_object_desc { get; set; }
        public string logDB_name { get; set; }
        public int intervals_per_day { get; set; }
        public Nullable<int> default_service_level_sec { get; set; }
        public Nullable<int> default_short_call_treshold { get; set; }
        public virtual acd_type acd_type { get; set; }
        public virtual ICollection<agent_info> agent_info { get; set; }
        public virtual ICollection<quality_info> quality_info { get; set; }
        public virtual ICollection<log_object_add_hours> log_object_add_hours { get; set; }
        public virtual ICollection<log_object_detail> log_object_detail { get; set; }
        public virtual ICollection<queue> queues { get; set; }
    }
}
