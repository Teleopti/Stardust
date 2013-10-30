using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class bridge_acd_login_person
    {
        public int acd_login_id { get; set; }
        public int person_id { get; set; }
        public Nullable<int> team_id { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_acd_login dim_acd_login { get; set; }
        public virtual dim_person dim_person { get; set; }
        public virtual dim_team dim_team { get; set; }
    }
}
