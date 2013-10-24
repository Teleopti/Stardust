using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_time_zone
    {
        public string time_zone_code { get; set; }
        public string time_zone_name { get; set; }
        public Nullable<bool> default_zone { get; set; }
        public Nullable<int> utc_conversion { get; set; }
        public Nullable<int> utc_conversion_dst { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
    }
}
