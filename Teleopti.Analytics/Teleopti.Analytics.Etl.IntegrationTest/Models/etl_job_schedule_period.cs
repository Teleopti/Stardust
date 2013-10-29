using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_job_schedule_period
    {
        public int schedule_id { get; set; }
        public int job_id { get; set; }
        public int relative_period_start { get; set; }
        public int relative_period_end { get; set; }
        public virtual etl_job etl_job { get; set; }
        public virtual etl_job_schedule etl_job_schedule { get; set; }
    }
}
