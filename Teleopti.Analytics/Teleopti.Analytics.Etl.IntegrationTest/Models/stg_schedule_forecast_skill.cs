using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_schedule_forecast_skill
    {
        public System.DateTime date { get; set; }
        public short interval_id { get; set; }
        public System.Guid skill_code { get; set; }
        public System.Guid scenario_code { get; set; }
        public Nullable<decimal> forecasted_resources_m { get; set; }
        public Nullable<decimal> forecasted_resources { get; set; }
        public Nullable<decimal> forecasted_resources_incl_shrinkage_m { get; set; }
        public Nullable<decimal> forecasted_resources_incl_shrinkage { get; set; }
        public Nullable<decimal> scheduled_resources_m { get; set; }
        public Nullable<decimal> scheduled_resources { get; set; }
        public Nullable<decimal> scheduled_resources_incl_shrinkage_m { get; set; }
        public Nullable<decimal> scheduled_resources_incl_shrinkage { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
    }
}
