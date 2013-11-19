using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_business_unit
    {
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
    }
}
