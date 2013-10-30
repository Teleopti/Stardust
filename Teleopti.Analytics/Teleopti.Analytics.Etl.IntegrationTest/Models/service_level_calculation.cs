using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class service_level_calculation
    {
        public int service_level_id { get; set; }
        public string service_level_name { get; set; }
        public string resource_key { get; set; }
    }
}
