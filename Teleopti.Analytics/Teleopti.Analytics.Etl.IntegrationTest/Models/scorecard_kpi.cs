using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class scorecard_kpi
    {
        public int scorecard_id { get; set; }
        public int kpi_id { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_kpi dim_kpi { get; set; }
        public virtual dim_scorecard dim_scorecard { get; set; }
    }
}
