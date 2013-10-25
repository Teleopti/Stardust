using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_shift_category
    {
        public dim_shift_category()
        {
            this.fact_schedule_day_count = new List<fact_schedule_day_count>();
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public int shift_category_id { get; set; }
        public Nullable<System.Guid> shift_category_code { get; set; }
        public string shift_category_name { get; set; }
        public string shift_category_shortname { get; set; }
        public Nullable<int> display_color { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
        public virtual ICollection<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
