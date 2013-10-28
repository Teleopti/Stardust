using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_job
    {
        public etl_job()
        {
            this.etl_job_execution = new List<etl_job_execution>();
            this.etl_job_schedule_period = new List<etl_job_schedule_period>();
        }

        public int job_id { get; set; }
        public string job_name { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual ICollection<etl_job_execution> etl_job_execution { get; set; }
        public virtual ICollection<etl_job_schedule_period> etl_job_schedule_period { get; set; }
    }
}
