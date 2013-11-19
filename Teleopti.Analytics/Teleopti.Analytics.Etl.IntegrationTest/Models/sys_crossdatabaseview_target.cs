using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class sys_crossdatabaseview_target
    {
        public sys_crossdatabaseview_target()
        {
            this.sys_crossdatabaseview = new List<sys_crossdatabaseview>();
        }

        public int target_id { get; set; }
        public string target_customName { get; set; }
        public string target_defaultName { get; set; }
        public bool confirmed { get; set; }
        public virtual ICollection<sys_crossdatabaseview> sys_crossdatabaseview { get; set; }
    }
}
