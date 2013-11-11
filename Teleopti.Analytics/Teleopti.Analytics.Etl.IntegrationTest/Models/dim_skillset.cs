using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_skillset
    {
        public dim_skillset()
        {
            this.bridge_skillset_skill = new List<bridge_skillset_skill>();
            this.dim_person = new List<dim_person>();
        }

        public int skillset_id { get; set; }
        public string skillset_code { get; set; }
        public string skillset_name { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public virtual ICollection<bridge_skillset_skill> bridge_skillset_skill { get; set; }
        public virtual ICollection<dim_person> dim_person { get; set; }
    }
}
