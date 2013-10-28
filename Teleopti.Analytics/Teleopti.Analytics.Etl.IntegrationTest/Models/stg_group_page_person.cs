using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_group_page_person
    {
        public System.Guid group_page_code { get; set; }
        public string group_page_name { get; set; }
        public string group_page_name_resource_key { get; set; }
        public System.Guid group_code { get; set; }
        public string group_name { get; set; }
        public bool group_is_custom { get; set; }
        public System.Guid person_code { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public int datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
    }
}
