using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class etl_jobstep
    {
        public etl_jobstep()
        {
            this.etl_jobstep_execution = new List<etl_jobstep_execution>();
        }

        public int jobstep_id { get; set; }
        public string jobstep_name { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public virtual ICollection<etl_jobstep_execution> etl_jobstep_execution { get; set; }
    }
}
