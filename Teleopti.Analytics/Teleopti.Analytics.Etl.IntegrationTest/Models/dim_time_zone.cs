using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_time_zone
    {
        public dim_time_zone()
        {
            this.bridge_time_zone = new List<bridge_time_zone>();
        }

        public short time_zone_id { get; set; }
        public string time_zone_code { get; set; }
        public string time_zone_name { get; set; }
        public Nullable<bool> default_zone { get; set; }
        public Nullable<int> utc_conversion { get; set; }
        public Nullable<int> utc_conversion_dst { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public bool to_be_deleted { get; set; }
        public Nullable<short> only_one_default_zone { get; set; }
        public virtual ICollection<bridge_time_zone> bridge_time_zone { get; set; }
    }
}
