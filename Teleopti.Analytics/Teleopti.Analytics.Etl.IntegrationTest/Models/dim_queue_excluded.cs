using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_queue_excluded
    {
        public int queue_original_id { get; set; }
        public short datasource_id { get; set; }
    }
}
