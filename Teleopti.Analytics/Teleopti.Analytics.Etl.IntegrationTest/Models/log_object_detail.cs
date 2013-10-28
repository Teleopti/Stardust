using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class log_object_detail
    {
        public int log_object_id { get; set; }
        public int detail_id { get; set; }
        public string detail_desc { get; set; }
        public string proc_name { get; set; }
        public Nullable<int> int_value { get; set; }
        public Nullable<System.DateTime> date_value { get; set; }
        public virtual log_object log_object { get; set; }
    }
}
