using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_requested_days
    {
        public System.Guid request_code { get; set; }
        public int person_id { get; set; }
        public int request_date_id { get; set; }
        public byte request_type_id { get; set; }
        public byte request_status_id { get; set; }
        public int request_day_count { get; set; }
        public short business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public Nullable<int> absence_id { get; set; }
        public virtual dim_absence dim_absence { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_person dim_person { get; set; }
        public virtual dim_request_status dim_request_status { get; set; }
        public virtual dim_request_type dim_request_type { get; set; }
    }
}
