using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_jobstep_error
    {
        public etl_jobstep_error()
        {
            this.etl_jobstep_execution = new List<etl_jobstep_execution>();
        }

        public int jobstep_error_id { get; set; }
        public string error_exception_message { get; set; }
        public string error_exception_stacktrace { get; set; }
        public string inner_error_exception_message { get; set; }
        public string inner_error_exception_stacktrace { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual ICollection<etl_jobstep_execution> etl_jobstep_execution { get; set; }
    }
}
