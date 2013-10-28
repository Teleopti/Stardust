using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_date
    {
        public dim_date()
        {
            this.bridge_time_zone = new List<bridge_time_zone>();
            this.bridge_time_zone1 = new List<bridge_time_zone>();
            this.fact_agent = new List<fact_agent>();
            this.fact_agent1 = new List<fact_agent>();
            this.fact_agent_queue = new List<fact_agent_queue>();
            this.fact_agent_queue1 = new List<fact_agent_queue>();
            this.fact_forecast_workload = new List<fact_forecast_workload>();
            this.fact_hourly_availability = new List<fact_hourly_availability>();
            this.fact_quality = new List<fact_quality>();
            this.fact_queue = new List<fact_queue>();
            this.fact_queue1 = new List<fact_queue>();
            this.fact_request = new List<fact_request>();
            this.fact_requested_days = new List<fact_requested_days>();
            this.fact_schedule_day_count = new List<fact_schedule_day_count>();
            this.fact_schedule_deviation = new List<fact_schedule_deviation>();
            this.fact_schedule = new List<fact_schedule>();
            this.fact_schedule1 = new List<fact_schedule>();
            this.fact_schedule2 = new List<fact_schedule>();
            this.fact_schedule3 = new List<fact_schedule>();
            this.fact_schedule4 = new List<fact_schedule>();
            this.fact_schedule_forecast_skill = new List<fact_schedule_forecast_skill>();
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public int date_id { get; set; }
        public System.DateTime date_date { get; set; }
        public int year { get; set; }
        public int year_month { get; set; }
        public int month { get; set; }
        public string month_name { get; set; }
        public string month_resource_key { get; set; }
        public int day_in_month { get; set; }
        public int weekday_number { get; set; }
        public string weekday_name { get; set; }
        public string weekday_resource_key { get; set; }
        public int week_number { get; set; }
        public string year_week { get; set; }
        public string quarter { get; set; }
        public System.DateTime insert_date { get; set; }
        public virtual ICollection<bridge_time_zone> bridge_time_zone { get; set; }
        public virtual ICollection<bridge_time_zone> bridge_time_zone1 { get; set; }
        public virtual ICollection<fact_agent> fact_agent { get; set; }
        public virtual ICollection<fact_agent> fact_agent1 { get; set; }
        public virtual ICollection<fact_agent_queue> fact_agent_queue { get; set; }
        public virtual ICollection<fact_agent_queue> fact_agent_queue1 { get; set; }
        public virtual ICollection<fact_forecast_workload> fact_forecast_workload { get; set; }
        public virtual ICollection<fact_hourly_availability> fact_hourly_availability { get; set; }
        public virtual ICollection<fact_quality> fact_quality { get; set; }
        public virtual ICollection<fact_queue> fact_queue { get; set; }
        public virtual ICollection<fact_queue> fact_queue1 { get; set; }
        public virtual ICollection<fact_request> fact_request { get; set; }
        public virtual ICollection<fact_requested_days> fact_requested_days { get; set; }
        public virtual ICollection<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public virtual ICollection<fact_schedule_deviation> fact_schedule_deviation { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule1 { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule2 { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule3 { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule4 { get; set; }
        public virtual ICollection<fact_schedule_forecast_skill> fact_schedule_forecast_skill { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
