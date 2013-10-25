using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class v_log_object
    {
        public int log_object_id { get; set; }
        public int acd_type_id { get; set; }
        public string log_object_desc { get; set; }
        public string logDB_name { get; set; }
        public int intervals_per_day { get; set; }
        public Nullable<int> default_service_level_sec { get; set; }
        public Nullable<int> default_short_call_treshold { get; set; }
    }
}
