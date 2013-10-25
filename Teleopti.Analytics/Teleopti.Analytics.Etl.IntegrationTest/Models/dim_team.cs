using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_team
    {
        public dim_team()
        {
            this.bridge_acd_login_person = new List<bridge_acd_login_person>();
            this.fact_kpi_targets_team = new List<fact_kpi_targets_team>();
        }

        public int team_id { get; set; }
        public Nullable<System.Guid> team_code { get; set; }
        public string team_name { get; set; }
        public Nullable<int> scorecard_id { get; set; }
        public Nullable<int> site_id { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual ICollection<bridge_acd_login_person> bridge_acd_login_person { get; set; }
        public virtual dim_scorecard dim_scorecard { get; set; }
        public virtual dim_site dim_site { get; set; }
        public virtual ICollection<fact_kpi_targets_team> fact_kpi_targets_team { get; set; }
    }
}
