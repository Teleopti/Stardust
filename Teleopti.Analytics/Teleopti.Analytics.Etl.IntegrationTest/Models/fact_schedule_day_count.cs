using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_schedule_day_count
    {
        public int date_id { get; set; }
        public short start_interval_id { get; set; }
        public int person_id { get; set; }
        public short scenario_id { get; set; }
        public Nullable<System.DateTime> starttime { get; set; }
        public int shift_category_id { get; set; }
        public int day_off_id { get; set; }
        public int absence_id { get; set; }
        public Nullable<int> day_count { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_absence dim_absence { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_day_off dim_day_off { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_person dim_person { get; set; }
        public virtual dim_scenario dim_scenario { get; set; }
        public virtual dim_shift_category dim_shift_category { get; set; }
    }
}
