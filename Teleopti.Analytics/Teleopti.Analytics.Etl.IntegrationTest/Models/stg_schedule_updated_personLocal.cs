using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_schedule_updated_personLocal
    {
        public int person_id { get; set; }
        public int time_zone_id { get; set; }
        public System.Guid person_code { get; set; }
        public System.DateTime valid_from_date_local { get; set; }
        public System.DateTime valid_to_date_local { get; set; }
    }
}
