using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_activity
    {
        public System.Guid activity_code { get; set; }
        public string activity_name { get; set; }
        public int display_color { get; set; }
        public string display_color_html { get; set; }
        public bool in_ready_time { get; set; }
        public Nullable<bool> in_contract_time { get; set; }
        public Nullable<bool> in_paid_time { get; set; }
        public Nullable<bool> in_work_time { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
    }
}
