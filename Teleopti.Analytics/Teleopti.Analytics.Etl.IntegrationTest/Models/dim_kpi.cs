using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_kpi
    {
        public dim_kpi()
        {
            this.fact_kpi_targets_team = new List<fact_kpi_targets_team>();
            this.scorecard_kpi = new List<scorecard_kpi>();
        }

        public int kpi_id { get; set; }
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
        public Nullable<bool> decreasing_value_is_positive { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual ICollection<fact_kpi_targets_team> fact_kpi_targets_team { get; set; }
        public virtual ICollection<scorecard_kpi> scorecard_kpi { get; set; }
    }
}
