using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_scorecard
    {
        public dim_scorecard()
        {
            this.dim_team = new List<dim_team>();
            this.scorecard_kpi = new List<scorecard_kpi>();
        }

        public int scorecard_id { get; set; }
        public Nullable<System.Guid> scorecard_code { get; set; }
        public string scorecard_name { get; set; }
        public int period { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual ICollection<dim_team> dim_team { get; set; }
        public virtual ICollection<scorecard_kpi> scorecard_kpi { get; set; }
    }
}
