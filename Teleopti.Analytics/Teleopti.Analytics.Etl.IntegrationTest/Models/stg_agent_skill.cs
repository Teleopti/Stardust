using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_agent_skill
    {
        public System.DateTime skill_date { get; set; }
        public int interval_id { get; set; }
        public System.Guid person_code { get; set; }
        public System.Guid skill_code { get; set; }
        public Nullable<System.DateTime> date_from { get; set; }
        public Nullable<System.DateTime> date_to { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
    }
}
