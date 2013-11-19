using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_time_zone_bridge
    {
        public System.DateTime date { get; set; }
        public int interval_id { get; set; }
        public string time_zone_code { get; set; }
        public Nullable<System.DateTime> local_date { get; set; }
        public Nullable<int> local_interval_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
    }
}
