using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class queue
    {
        public queue()
        {
            this.agent_logg = new List<agent_logg>();
            this.queue_logg = new List<queue_logg>();
        }

        public int queue1 { get; set; }
        public string orig_desc { get; set; }
        public int log_object_id { get; set; }
        public Nullable<int> orig_queue_id { get; set; }
        public string display_desc { get; set; }
        public virtual ICollection<agent_logg> agent_logg { get; set; }
        public virtual log_object log_object { get; set; }
        public virtual ICollection<queue_logg> queue_logg { get; set; }
    }
}
