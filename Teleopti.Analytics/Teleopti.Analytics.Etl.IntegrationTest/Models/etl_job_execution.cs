using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_job_execution
    {
        public etl_job_execution()
        {
            this.etl_jobstep_execution = new List<etl_jobstep_execution>();
        }

        public int job_execution_id { get; set; }
        public Nullable<int> job_id { get; set; }
        public Nullable<int> schedule_id { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public Nullable<System.DateTime> job_start_time { get; set; }
        public Nullable<System.DateTime> job_end_time { get; set; }
        public Nullable<int> duration_s { get; set; }
        public Nullable<int> affected_rows { get; set; }
        public Nullable<bool> job_execution_success { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual etl_job etl_job { get; set; }
        public virtual etl_job_schedule etl_job_schedule { get; set; }
        public virtual ICollection<etl_jobstep_execution> etl_jobstep_execution { get; set; }
    }
}
