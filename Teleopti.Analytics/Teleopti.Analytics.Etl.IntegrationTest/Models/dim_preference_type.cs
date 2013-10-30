using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_preference_type
    {
        public dim_preference_type()
        {
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public int preference_type_id { get; set; }
        public string preference_type_name { get; set; }
        public string resource_key { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
