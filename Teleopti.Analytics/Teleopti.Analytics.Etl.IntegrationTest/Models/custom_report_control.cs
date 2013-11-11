using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class custom_report_control
    {
        public custom_report_control()
        {
            this.custom_report_control_collection = new List<custom_report_control_collection>();
        }

        public string control_name { get; set; }
        public string fill_proc_name { get; set; }
        public System.Guid Id { get; set; }
        public virtual ICollection<custom_report_control_collection> custom_report_control_collection { get; set; }
    }
}
