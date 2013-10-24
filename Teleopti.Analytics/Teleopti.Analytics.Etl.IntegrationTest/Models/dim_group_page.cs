using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_group_page
    {
        public dim_group_page()
        {
            this.bridge_group_page_person = new List<bridge_group_page_person>();
        }

        public int group_page_id { get; set; }
        public System.Guid group_page_code { get; set; }
        public string group_page_name { get; set; }
        public string group_page_name_resource_key { get; set; }
        public int group_id { get; set; }
        public System.Guid group_code { get; set; }
        public string group_name { get; set; }
        public bool group_is_custom { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public int datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual ICollection<bridge_group_page_person> bridge_group_page_person { get; set; }
    }
}
