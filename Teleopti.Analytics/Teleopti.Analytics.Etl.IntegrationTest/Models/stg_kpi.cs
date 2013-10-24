using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_kpi
    {
        public System.Guid kpi_code { get; set; }
        public string kpi_name { get; set; }
        public string resource_key { get; set; }
        public int target_value_type { get; set; }
        public double default_target_value { get; set; }
        public double default_min_value { get; set; }
        public double default_max_value { get; set; }
        public int default_between_color { get; set; }
        public int default_lower_than_min_color { get; set; }
        public int default_higher_than_max_color { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
    }
}
