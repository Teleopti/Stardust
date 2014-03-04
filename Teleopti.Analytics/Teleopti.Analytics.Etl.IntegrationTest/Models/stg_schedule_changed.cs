using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_schedule_changed
    {
        public System.DateTime schedule_date_local { get; set; }
        public System.Guid person_code { get; set; }
        public System.Guid scenario_code { get; set; }
        public System.Guid business_unit_code { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime datasource_update_date { get; set; }
    }
}
