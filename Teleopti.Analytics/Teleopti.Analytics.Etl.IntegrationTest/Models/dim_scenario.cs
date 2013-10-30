using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_scenario
    {
        public dim_scenario()
        {
            this.fact_forecast_workload = new List<fact_forecast_workload>();
            this.fact_hourly_availability = new List<fact_hourly_availability>();
            this.fact_schedule_day_count = new List<fact_schedule_day_count>();
            this.fact_schedule = new List<fact_schedule>();
            this.fact_schedule_forecast_skill = new List<fact_schedule_forecast_skill>();
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public short scenario_id { get; set; }
        public Nullable<System.Guid> scenario_code { get; set; }
        public string scenario_name { get; set; }
        public Nullable<bool> default_scenario { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
        public virtual ICollection<fact_forecast_workload> fact_forecast_workload { get; set; }
        public virtual ICollection<fact_hourly_availability> fact_hourly_availability { get; set; }
        public virtual ICollection<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule { get; set; }
        public virtual ICollection<fact_schedule_forecast_skill> fact_schedule_forecast_skill { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
