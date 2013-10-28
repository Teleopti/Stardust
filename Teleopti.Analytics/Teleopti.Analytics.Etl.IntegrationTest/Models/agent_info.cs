using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class agent_info
    {
        public agent_info()
        {
            this.agent_logg = new List<agent_logg>();
            this.quality_logg = new List<quality_logg>();
        }

        public int Agent_id { get; set; }
        public string Agent_name { get; set; }
        public Nullable<bool> is_active { get; set; }
        public int log_object_id { get; set; }
        public string orig_agent_id { get; set; }
        public virtual log_object log_object { get; set; }
        public virtual ICollection<agent_logg> agent_logg { get; set; }
        public virtual ICollection<quality_logg> quality_logg { get; set; }
    }
}
