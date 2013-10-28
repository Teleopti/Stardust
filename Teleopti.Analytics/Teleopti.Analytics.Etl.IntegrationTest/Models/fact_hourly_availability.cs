using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_hourly_availability
    {
        public int date_id { get; set; }
        public int person_id { get; set; }
        public short scenario_id { get; set; }
        public Nullable<int> available_time_m { get; set; }
        public Nullable<int> available_days { get; set; }
        public Nullable<int> scheduled_time_m { get; set; }
        public Nullable<int> scheduled_days { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_person dim_person { get; set; }
        public virtual dim_scenario dim_scenario { get; set; }
    }
}
