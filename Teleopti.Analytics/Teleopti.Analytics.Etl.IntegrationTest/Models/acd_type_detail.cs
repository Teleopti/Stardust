using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class acd_type_detail
    {
        public int acd_type_id { get; set; }
        public int detail_id { get; set; }
        public string detail_name { get; set; }
        public string proc_name { get; set; }
        public virtual acd_type acd_type { get; set; }
    }
}
