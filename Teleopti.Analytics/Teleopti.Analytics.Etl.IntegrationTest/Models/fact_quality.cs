using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_quality
    {
        public int date_id { get; set; }
        public int acd_login_id { get; set; }
        public int evaluation_id { get; set; }
        public int quality_quest_id { get; set; }
        public Nullable<decimal> score { get; set; }
        public int datasource_id { get; set; }
        public virtual dim_acd_login dim_acd_login { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_quality_quest dim_quality_quest { get; set; }
    }
}
