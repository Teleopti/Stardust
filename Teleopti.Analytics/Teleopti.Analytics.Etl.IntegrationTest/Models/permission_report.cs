using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class permission_report
    {
        public System.Guid person_code { get; set; }
        public int team_id { get; set; }
        public bool my_own { get; set; }
        public int business_unit_id { get; set; }
        public short datasource_id { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public System.Guid ReportId { get; set; }
    }
}
