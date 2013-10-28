using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_hourly_availability
    {
        public System.DateTime restriction_date { get; set; }
        public System.Guid person_code { get; set; }
        public System.Guid scenario_code { get; set; }
        public int available_time_m { get; set; }
        public int scheduled_time_m { get; set; }
        public short scheduled { get; set; }
        public System.Guid business_unit_code { get; set; }
        public short datasource_id { get; set; }
    }
}
