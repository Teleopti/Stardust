using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_workload
    {
        public System.Guid workload_code { get; set; }
        public string workload_name { get; set; }
        public Nullable<System.Guid> skill_code { get; set; }
        public string skill_name { get; set; }
        public string time_zone_code { get; set; }
        public Nullable<System.Guid> forecast_method_code { get; set; }
        public string forecast_method_name { get; set; }
        public double percentage_offered { get; set; }
        public double percentage_overflow_in { get; set; }
        public double percentage_overflow_out { get; set; }
        public double percentage_abandoned { get; set; }
        public double percentage_abandoned_short { get; set; }
        public double percentage_abandoned_within_service_level { get; set; }
        public double percentage_abandoned_after_service_level { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public bool skill_is_deleted { get; set; }
        public bool is_deleted { get; set; }
    }
}
