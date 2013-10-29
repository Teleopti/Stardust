using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_job_schedule
    {
        public etl_job_schedule()
        {
            this.etl_job_execution = new List<etl_job_execution>();
            this.etl_job_schedule_period = new List<etl_job_schedule_period>();
        }

        public int schedule_id { get; set; }
        public string schedule_name { get; set; }
        public bool enabled { get; set; }
        public int schedule_type { get; set; }
        public int occurs_daily_at { get; set; }
        public int occurs_every_minute { get; set; }
        public int recurring_starttime { get; set; }
        public int recurring_endtime { get; set; }
        public string etl_job_name { get; set; }
        public Nullable<int> etl_relative_period_start { get; set; }
        public Nullable<int> etl_relative_period_end { get; set; }
        public Nullable<int> etl_datasource_id { get; set; }
        public string description { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual ICollection<etl_job_execution> etl_job_execution { get; set; }
        public virtual ICollection<etl_job_schedule_period> etl_job_schedule_period { get; set; }
    }
}
