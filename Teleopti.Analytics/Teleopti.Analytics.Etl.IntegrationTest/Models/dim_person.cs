using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_person
    {
        public dim_person()
        {
            this.bridge_acd_login_person = new List<bridge_acd_login_person>();
            this.bridge_group_page_person = new List<bridge_group_page_person>();
            this.fact_hourly_availability = new List<fact_hourly_availability>();
            this.fact_request = new List<fact_request>();
            this.fact_requested_days = new List<fact_requested_days>();
            this.fact_schedule_day_count = new List<fact_schedule_day_count>();
            this.fact_schedule_deviation = new List<fact_schedule_deviation>();
            this.fact_schedule = new List<fact_schedule>();
            this.fact_schedule_preference = new List<fact_schedule_preference>();
        }

        public int person_id { get; set; }
        public Nullable<System.Guid> person_code { get; set; }
        public System.DateTime valid_from_date { get; set; }
        public System.DateTime valid_to_date { get; set; }
        public int valid_from_date_id { get; set; }
        public int valid_from_interval_id { get; set; }
        public int valid_to_date_id { get; set; }
        public int valid_to_interval_id { get; set; }
        public Nullable<System.Guid> person_period_code { get; set; }
        public string person_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string employment_number { get; set; }
        public Nullable<int> employment_type_code { get; set; }
        public string employment_type_name { get; set; }
        public Nullable<System.Guid> contract_code { get; set; }
        public string contract_name { get; set; }
        public Nullable<System.Guid> parttime_code { get; set; }
        public string parttime_percentage { get; set; }
        public Nullable<int> team_id { get; set; }
        public Nullable<System.Guid> team_code { get; set; }
        public string team_name { get; set; }
        public Nullable<int> site_id { get; set; }
        public Nullable<System.Guid> site_code { get; set; }
        public string site_name { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public Nullable<int> skillset_id { get; set; }
        public string email { get; set; }
        public string note { get; set; }
        public Nullable<System.DateTime> employment_start_date { get; set; }
        public Nullable<System.DateTime> employment_end_date { get; set; }
        public Nullable<int> time_zone_id { get; set; }
        public Nullable<bool> is_agent { get; set; }
        public Nullable<bool> is_user { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public bool to_be_deleted { get; set; }
        public string windows_domain { get; set; }
        public string windows_username { get; set; }
        public virtual ICollection<bridge_acd_login_person> bridge_acd_login_person { get; set; }
        public virtual ICollection<bridge_group_page_person> bridge_group_page_person { get; set; }
        public virtual dim_skillset dim_skillset { get; set; }
        public virtual ICollection<fact_hourly_availability> fact_hourly_availability { get; set; }
        public virtual ICollection<fact_request> fact_request { get; set; }
        public virtual ICollection<fact_requested_days> fact_requested_days { get; set; }
        public virtual ICollection<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public virtual ICollection<fact_schedule_deviation> fact_schedule_deviation { get; set; }
        public virtual ICollection<fact_schedule> fact_schedule { get; set; }
        public virtual ICollection<fact_schedule_preference> fact_schedule_preference { get; set; }
    }
}
