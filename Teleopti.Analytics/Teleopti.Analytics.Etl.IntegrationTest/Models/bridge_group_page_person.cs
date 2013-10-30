using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class bridge_group_page_person
    {
        public int group_page_id { get; set; }
        public int person_id { get; set; }
        public int datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public virtual dim_group_page dim_group_page { get; set; }
        public virtual dim_person dim_person { get; set; }
    }
}
