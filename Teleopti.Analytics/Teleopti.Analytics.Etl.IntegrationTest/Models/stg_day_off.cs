using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_day_off
    {
        public Nullable<System.Guid> day_off_code { get; set; }
        public string day_off_name { get; set; }
        public string day_off_shortname { get; set; }
        public int display_color { get; set; }
        public string display_color_html { get; set; }
        public System.Guid business_unit_code { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
    }
}
