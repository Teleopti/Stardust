using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class log_object_add_hours
    {
        public int log_object_id { get; set; }
        public System.DateTime datetime_from { get; set; }
        public System.DateTime datetime_to { get; set; }
        public int add_hours { get; set; }
        public virtual log_object log_object { get; set; }
    }
}
