using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_shift_category
    {
        public System.Guid shift_category_code { get; set; }
        public string shift_category_name { get; set; }
        public string shift_category_shortname { get; set; }
        public int display_color { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public bool is_deleted { get; set; }
    }
}
