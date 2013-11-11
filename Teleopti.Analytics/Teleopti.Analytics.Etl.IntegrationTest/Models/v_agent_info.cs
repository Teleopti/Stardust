using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class v_agent_info
    {
        public int Agent_id { get; set; }
        public string Agent_name { get; set; }
        public Nullable<bool> is_active { get; set; }
        public int log_object_id { get; set; }
        public string orig_agent_id { get; set; }
    }
}
