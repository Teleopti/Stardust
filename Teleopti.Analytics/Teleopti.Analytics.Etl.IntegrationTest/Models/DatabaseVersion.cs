using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class DatabaseVersion
    {
        public int BuildNumber { get; set; }
        public string SystemVersion { get; set; }
        public System.DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
    }
}
