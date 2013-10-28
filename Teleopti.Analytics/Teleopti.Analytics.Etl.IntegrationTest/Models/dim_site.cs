using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_site
    {
        public dim_site()
        {
            this.dim_team = new List<dim_team>();
        }

        public int site_id { get; set; }
        public Nullable<System.Guid> site_code { get; set; }
        public string site_name { get; set; }
        public Nullable<int> business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_business_unit dim_business_unit { get; set; }
        public virtual ICollection<dim_team> dim_team { get; set; }
    }
}
