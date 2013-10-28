using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class quality_info
    {
        public quality_info()
        {
            this.quality_logg = new List<quality_logg>();
        }

        public int quality_id { get; set; }
        public string quality_name { get; set; }
        public string quality_type { get; set; }
        public Nullable<float> score_weight { get; set; }
        public int log_object_id { get; set; }
        public int original_id { get; set; }
        public virtual log_object log_object { get; set; }
        public virtual ICollection<quality_logg> quality_logg { get; set; }
    }
}
