using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class report_control
    {
        public report_control()
        {
            this.report_control_collection = new List<report_control_collection>();
        }

        public System.Guid Id { get; set; }
        public int control_id { get; set; }
        public string control_name { get; set; }
        public string fill_proc_name { get; set; }
        public virtual ICollection<report_control_collection> report_control_collection { get; set; }
    }
}
