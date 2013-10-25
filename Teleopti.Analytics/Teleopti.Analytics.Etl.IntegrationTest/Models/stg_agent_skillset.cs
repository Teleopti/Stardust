using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_agent_skillset
    {
        public System.Guid person_code { get; set; }
        public int skill_id { get; set; }
        public System.DateTime date_from { get; set; }
        public int skillset_id { get; set; }
        public Nullable<System.DateTime> date_to { get; set; }
        public string skill_name { get; set; }
        public string skill_sum_code { get; set; }
        public string skill_sum_name { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
    }
}
