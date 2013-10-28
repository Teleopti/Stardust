using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class report
    {
        public System.Guid Id { get; set; }
        public int report_id { get; set; }
        public int control_collection_id { get; set; }
        public string url { get; set; }
        public string target { get; set; }
        public string report_name { get; set; }
        public string report_name_resource_key { get; set; }
        public bool visible { get; set; }
        public string rpt_file_name { get; set; }
        public string proc_name { get; set; }
        public string help_key { get; set; }
        public string sub1_name { get; set; }
        public string sub1_proc_name { get; set; }
        public string sub2_name { get; set; }
        public string sub2_proc_name { get; set; }
        public System.Guid ControlCollectionId { get; set; }
    }
}
