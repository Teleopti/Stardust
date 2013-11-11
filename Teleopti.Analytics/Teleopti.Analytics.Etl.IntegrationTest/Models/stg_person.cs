using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_person
    {
        public System.Guid person_code { get; set; }
        public System.DateTime valid_from_date { get; set; }
        public System.DateTime valid_to_date { get; set; }
        public int valid_from_interval_id { get; set; }
        public int valid_to_interval_id { get; set; }
        public Nullable<System.DateTime> valid_to_interval_start { get; set; }
        public Nullable<System.Guid> person_period_code { get; set; }
        public string person_name { get; set; }
        public string person_first_name { get; set; }
        public string person_last_name { get; set; }
        public System.Guid team_code { get; set; }
        public string team_name { get; set; }
        public Nullable<System.Guid> scorecard_code { get; set; }
        public System.Guid site_code { get; set; }
        public string site_name { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public string email { get; set; }
        public string note { get; set; }
        public string employment_number { get; set; }
        public System.DateTime employment_start_date { get; set; }
        public System.DateTime employment_end_date { get; set; }
        public string time_zone_code { get; set; }
        public Nullable<bool> is_agent { get; set; }
        public Nullable<bool> is_user { get; set; }
        public Nullable<System.Guid> contract_code { get; set; }
        public string contract_name { get; set; }
        public Nullable<System.Guid> parttime_code { get; set; }
        public string parttime_percentage { get; set; }
        public string employment_type { get; set; }
        public short datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public string windows_domain { get; set; }
        public string windows_username { get; set; }
    }
}
