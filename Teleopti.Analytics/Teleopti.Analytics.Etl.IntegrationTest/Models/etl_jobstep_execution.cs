using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_jobstep_execution
    {
        public int jobstep_execution_id { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public Nullable<int> duration_s { get; set; }
        public Nullable<int> rows_affected { get; set; }
        public Nullable<int> job_execution_id { get; set; }
        public Nullable<int> jobstep_error_id { get; set; }
        public Nullable<int> jobstep_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual etl_job_execution etl_job_execution { get; set; }
        public virtual etl_jobstep etl_jobstep { get; set; }
        public virtual etl_jobstep_error etl_jobstep_error { get; set; }
    }
}
