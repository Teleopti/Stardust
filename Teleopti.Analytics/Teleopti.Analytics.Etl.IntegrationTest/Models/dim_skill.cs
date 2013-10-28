using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_skill
    {
        public dim_skill()
        {
            this.bridge_queue_workload = new List<bridge_queue_workload>();
            this.bridge_skillset_skill = new List<bridge_skillset_skill>();
            this.fact_forecast_workload = new List<fact_forecast_workload>();
            this.fact_schedule_forecast_skill = new List<fact_schedule_forecast_skill>();
        }

        public int skill_id { get; set; }
        public Nullable<System.Guid> skill_code { get; set; }
        public string skill_name { get; set; }
        public int time_zone_id { get; set; }
        public Nullable<System.Guid> forecast_method_code { get; set; }
        public string forecast_method_name { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
        public virtual ICollection<bridge_queue_workload> bridge_queue_workload { get; set; }
        public virtual ICollection<bridge_skillset_skill> bridge_skillset_skill { get; set; }
        public virtual ICollection<fact_forecast_workload> fact_forecast_workload { get; set; }
        public virtual ICollection<fact_schedule_forecast_skill> fact_schedule_forecast_skill { get; set; }
    }
}
