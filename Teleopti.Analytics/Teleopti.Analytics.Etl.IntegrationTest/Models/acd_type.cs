using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class acd_type
    {
        public acd_type()
        {
            this.acd_type_detail = new List<acd_type_detail>();
            this.log_object = new List<log_object>();
        }

        public int acd_type_id { get; set; }
        public string acd_type_desc { get; set; }
        public virtual ICollection<acd_type_detail> acd_type_detail { get; set; }
        public virtual ICollection<log_object> log_object { get; set; }
    }
}
