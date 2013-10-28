using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_interval
    {
        public dim_interval()
        {
            this.bridge_time_zone = new List<bridge_time_zone>();
            this.bridge_time_zone1 = new List<bridge_time_zone>();
            this.fact_agent = new List<fact_agent>();
            this.fact_agent1 = new List<fact_agent>();
            this.fact_agent_queue = new List<fact_agent_queue>();
            this.fact_agent_queue1 = new List<fact_agent_queue>();
            this.fact_forecast_workload = new List<fact_forecast_workload>();
            this.fact_queue = new List<fact_queue>();
            this.fact_queue1 = new List<fact_queue>();
            this.fact_schedule_day_count = new List<fact_schedule_day_count>();
            this.fact_schedule_deviation = new List<fact_schedule_deviation>();
            this.fact_schedule = new List<fact_schedule>();
            this.fact_schedule1 = new List<fact_schedule>();
            this.fact_schedule_forecast_skill = new List<fact_schedule_forecast_skill>();
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public short interval_id { get; set; }
        public string interval_name { get; set; }
        public string halfhour_name { get; set; }
        public string hour_name { get; set; }
        public Nullable<System.DateTime> interval_start { get; set; }
        public Nullable<System.DateTime> interval_end { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual ICollection<bridge_time_zone> bridge_time_zone { get; set; }
        public virtual ICollection<bridge_time_zone> bridge_time_zone1 { get; set; }
        public virtual ICollection<fact_agent> fact_agent { get; set; }
        public virtual ICollection<fact_agent> fact_agent1 { get; set; }
        public virtual ICollection<fact_agent_queue> fact_agent_queue { get; set; }
        public virtual ICollection<fact_agent_queue> fact_agent_queue1 { get; set; }
        public virtual ICollection<fact_forecast_workload> fact_forecast_workload { get; set; }
        public virtual ICollection<fact_queue> fact_queue { get; set; }
        public virtual ICollection<fact_queue> fact_queue1 { get; set; }
        public virtual ICollection<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public virtual ICollection<fact_schedule_deviation> fact_schedule_deviation { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule1 { get; set; }
        public virtual ICollection<fact_schedule_forecast_skill> fact_schedule_forecast_skill { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
