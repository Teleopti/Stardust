using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class sys_crossdatabaseview
    {
        public int view_id { get; set; }
        public string view_name { get; set; }
        public string view_definition { get; set; }
        public int target_id { get; set; }
        public virtual sys_crossdatabaseview_target sys_crossdatabaseview_target { get; set; }
    }
}
