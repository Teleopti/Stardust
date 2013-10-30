using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_schedule_forecast_skill
    {
        public int date_id { get; set; }
        public short interval_id { get; set; }
        public int skill_id { get; set; }
        public short scenario_id { get; set; }
        public Nullable<decimal> forecasted_resources_m { get; set; }
        public Nullable<decimal> forecasted_resources { get; set; }
        public Nullable<decimal> forecasted_resources_incl_shrinkage_m { get; set; }
        public Nullable<decimal> forecasted_resources_incl_shrinkage { get; set; }
        public Nullable<decimal> scheduled_resources_m { get; set; }
        public Nullable<decimal> scheduled_resources { get; set; }
        public Nullable<decimal> scheduled_resources_incl_shrinkage_m { get; set; }
        public Nullable<decimal> scheduled_resources_incl_shrinkage { get; set; }
        public Nullable<decimal> intraday_deviation_m { get; set; }
        public Nullable<decimal> relative_difference { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_scenario dim_scenario { get; set; }
        public virtual dim_skill dim_skill { get; set; }
    }
}
