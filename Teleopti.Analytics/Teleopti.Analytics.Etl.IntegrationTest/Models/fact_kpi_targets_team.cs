using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_kpi_targets_team
    {
        public int kpi_id { get; set; }
        public int team_id { get; set; }
        public int business_unit_id { get; set; }
        public double target_value { get; set; }
        public double min_value { get; set; }
        public double max_value { get; set; }
        public int between_color { get; set; }
        public int lower_than_min_color { get; set; }
        public int higher_than_max_color { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_business_unit dim_business_unit { get; set; }
        public virtual dim_kpi dim_kpi { get; set; }
        public virtual dim_team dim_team { get; set; }
    }
}
