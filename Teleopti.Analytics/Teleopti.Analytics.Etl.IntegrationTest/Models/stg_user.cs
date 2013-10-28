using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_user
    {
        public System.Guid person_code { get; set; }
        public string person_first_name { get; set; }
        public string person_last_name { get; set; }
        public string application_logon_name { get; set; }
        public string windows_logon_name { get; set; }
        public string windows_domain_name { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public Nullable<int> language_id { get; set; }
        public string language_name { get; set; }
        public string culture { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
    }
}
