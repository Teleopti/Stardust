using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_job_delayed
    {
        public int Id { get; set; }
        public string stored_procedured { get; set; }
        public string parameter_string { get; set; }
        public System.DateTime insert_date { get; set; }
        public Nullable<System.DateTime> execute_date { get; set; }
    }
}
