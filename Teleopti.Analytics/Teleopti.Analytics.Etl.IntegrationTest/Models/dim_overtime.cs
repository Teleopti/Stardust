using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_overtime
    {
        public int overtime_id { get; set; }
        public Nullable<System.Guid> overtime_code { get; set; }
        public string overtime_name { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
    }
}
