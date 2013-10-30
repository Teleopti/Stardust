using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_queue
    {
        public dim_queue()
        {
            this.bridge_queue_workload = new List<bridge_queue_workload>();
            this.fact_agent_queue = new List<fact_agent_queue>();
            this.fact_queue = new List<fact_queue>();
        }

        public int queue_id { get; set; }
        public Nullable<int> queue_agg_id { get; set; }
        public string queue_original_id { get; set; }
        public string queue_name { get; set; }
        public string queue_description { get; set; }
        public string log_object_name { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual ICollection<bridge_queue_workload> bridge_queue_workload { get; set; }
        public virtual ICollection<fact_agent_queue> fact_agent_queue { get; set; }
        public virtual ICollection<fact_queue> fact_queue { get; set; }
    }
}
