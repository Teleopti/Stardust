using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_activity
    {
        public dim_activity()
        {
            this.fact_schedule = new List<fact_schedule>();
        }

        public int activity_id { get; set; }
        public Nullable<System.Guid> activity_code { get; set; }
        public string activity_name { get; set; }
        public int display_color { get; set; }
        public bool in_ready_time { get; set; }
        public string in_ready_time_name { get; set; }
        public Nullable<bool> in_contract_time { get; set; }
        public string in_contract_time_name { get; set; }
        public Nullable<bool> in_paid_time { get; set; }
        public string in_paid_time_name { get; set; }
        public Nullable<bool> in_work_time { get; set; }
        public string in_work_time_name { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
        public string display_color_html { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule { get; set; }
    }
}
