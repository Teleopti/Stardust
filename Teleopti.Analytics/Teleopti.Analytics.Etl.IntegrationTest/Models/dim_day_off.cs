using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_day_off
    {
        public dim_day_off()
        {
            this.fact_schedule_day_count = new List<fact_schedule_day_count>();
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public int day_off_id { get; set; }
        public Nullable<System.Guid> day_off_code { get; set; }
        public string day_off_name { get; set; }
        public Nullable<int> display_color { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public string display_color_html { get; set; }
        public string day_off_shortname { get; set; }
        public virtual ICollection<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
