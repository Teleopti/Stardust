using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_skill
    {
        public System.Guid skill_code { get; set; }
        public string skill_name { get; set; }
        public string time_zone_code { get; set; }
        public Nullable<System.Guid> forecast_method_code { get; set; }
        public string forecast_method_name { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
    }
}
