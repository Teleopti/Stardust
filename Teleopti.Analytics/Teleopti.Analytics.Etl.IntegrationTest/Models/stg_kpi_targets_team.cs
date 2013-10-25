using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_kpi_targets_team
    {
        public System.Guid kpi_code { get; set; }
        public System.Guid team_code { get; set; }
        public double target_value { get; set; }
        public double min_value { get; set; }
        public double max_value { get; set; }
        public int between_color { get; set; }
        public int lower_than_min_color { get; set; }
        public int higher_than_max_color { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
    }
}
