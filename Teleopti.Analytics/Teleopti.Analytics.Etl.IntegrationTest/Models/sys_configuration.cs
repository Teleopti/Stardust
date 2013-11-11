using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class sys_configuration
    {
        public string key { get; set; }
        public string value { get; set; }
        public System.DateTime insert_date { get; set; }
    }
}
