using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class bridge_time_zone
    {
        public int date_id { get; set; }
        public short interval_id { get; set; }
        public short time_zone_id { get; set; }
        public Nullable<int> local_date_id { get; set; }
        public Nullable<short> local_interval_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_date dim_date1 { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_interval dim_interval1 { get; set; }
        public virtual dim_time_zone dim_time_zone { get; set; }
    }
}
