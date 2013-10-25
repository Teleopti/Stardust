using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_permission_report
    {
        public Nullable<System.Guid> person_code { get; set; }
        public Nullable<System.Guid> ReportId { get; set; }
        public Nullable<System.Guid> team_id { get; set; }
        public Nullable<bool> my_own { get; set; }
        public System.Guid business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
    }
}
