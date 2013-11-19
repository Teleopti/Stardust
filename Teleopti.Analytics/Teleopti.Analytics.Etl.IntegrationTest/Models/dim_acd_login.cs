using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_acd_login
    {
        public dim_acd_login()
        {
            this.bridge_acd_login_person = new List<bridge_acd_login_person>();
            this.fact_agent = new List<fact_agent>();
            this.fact_agent_queue = new List<fact_agent_queue>();
            this.fact_quality = new List<fact_quality>();
        }

        public int acd_login_id { get; set; }
        public Nullable<int> acd_login_agg_id { get; set; }
        public string acd_login_original_id { get; set; }
        public string acd_login_name { get; set; }
        public string log_object_name { get; set; }
        public Nullable<bool> is_active { get; set; }
        public int time_zone_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual ICollection<bridge_acd_login_person> bridge_acd_login_person { get; set; }
        public virtual ICollection<fact_agent> fact_agent { get; set; }
        public virtual ICollection<fact_agent_queue> fact_agent_queue { get; set; }
        public virtual ICollection<fact_quality> fact_quality { get; set; }
    }
}
