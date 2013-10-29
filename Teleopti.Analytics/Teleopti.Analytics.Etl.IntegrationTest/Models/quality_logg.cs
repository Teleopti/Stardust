using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class quality_logg
    {
        public int quality_id { get; set; }
        public System.DateTime date_from { get; set; }
        public int agent_id { get; set; }
        public int evaluation_id { get; set; }
        public Nullable<float> score { get; set; }
        public virtual agent_info agent_info { get; set; }
        public virtual quality_info quality_info { get; set; }
    }
}
