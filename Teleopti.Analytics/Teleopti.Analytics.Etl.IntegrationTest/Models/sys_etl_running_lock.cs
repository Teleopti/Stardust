using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class sys_etl_running_lock
    {
        public int id { get; set; }
        public string computer_name { get; set; }
        public System.DateTime start_time { get; set; }
        public string job_name { get; set; }
        public bool is_started_by_service { get; set; }
        public System.DateTime lock_until { get; set; }
    }
}
