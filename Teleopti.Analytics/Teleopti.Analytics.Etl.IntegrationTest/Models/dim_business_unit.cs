using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_business_unit
    {
        public dim_business_unit()
        {
            this.dim_site = new List<dim_site>();
            this.fact_kpi_targets_team = new List<fact_kpi_targets_team>();
        }

        public int business_unit_id { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual ICollection<dim_site> dim_site { get; set; }
        public virtual ICollection<fact_kpi_targets_team> fact_kpi_targets_team { get; set; }
    }
}
