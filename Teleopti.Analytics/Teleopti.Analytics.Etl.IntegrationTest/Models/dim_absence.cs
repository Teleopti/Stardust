using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_absence
    {
        public dim_absence()
        {
            this.fact_request = new List<fact_request>();
            this.fact_requested_days = new List<fact_requested_days>();
            this.fact_schedule_day_count = new List<fact_schedule_day_count>();
            this.fact_schedule = new List<fact_schedule>();
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public int absence_id { get; set; }
        public Nullable<System.Guid> absence_code { get; set; }
        public string absence_name { get; set; }
        public int display_color { get; set; }
        public Nullable<bool> in_contract_time { get; set; }
        public string in_contract_time_name { get; set; }
        public Nullable<bool> in_paid_time { get; set; }
        public string in_paid_time_name { get; set; }
        public Nullable<bool> in_work_time { get; set; }
        public string in_work_time_name { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
        public string display_color_html { get; set; }
        public string absence_shortname { get; set; }
        public virtual ICollection<fact_request> fact_request { get; set; }
        public virtual ICollection<fact_requested_days> fact_requested_days { get; set; }
        public virtual ICollection<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
