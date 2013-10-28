using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class v_bridge_acd_login_person_date
    {
        public int date_id { get; set; }
        public int acd_login_id { get; set; }
        public int person_id { get; set; }
        public int valid_from_date_id { get; set; }
        public int valid_to_date_id { get; set; }
        public Nullable<int> team_id { get; set; }
        public Nullable<int> business_unit_id { get; set; }
    }
}
