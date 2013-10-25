using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class bridge_queue_workload
    {
        public int queue_id { get; set; }
        public int workload_id { get; set; }
        public Nullable<int> skill_id { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_queue dim_queue { get; set; }
        public virtual dim_skill dim_skill { get; set; }
        public virtual dim_workload dim_workload { get; set; }
    }
}
