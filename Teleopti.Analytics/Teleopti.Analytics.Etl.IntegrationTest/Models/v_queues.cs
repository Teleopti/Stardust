using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class v_queues
    {
        public int queue { get; set; }
        public string orig_desc { get; set; }
        public int log_object_id { get; set; }
        public Nullable<int> orig_queue_id { get; set; }
        public string display_desc { get; set; }
    }
}
