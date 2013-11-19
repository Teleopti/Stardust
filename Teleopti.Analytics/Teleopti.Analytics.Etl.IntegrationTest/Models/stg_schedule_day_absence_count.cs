using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_schedule_day_absence_count
    {
        public System.DateTime date { get; set; }
        public short start_interval_id { get; set; }
        public System.Guid person_code { get; set; }
        public System.Guid scenario_code { get; set; }
        public Nullable<System.DateTime> starttime { get; set; }
        public System.Guid absence_code { get; set; }
        public Nullable<int> day_count { get; set; }
        public System.Guid business_unit_code { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
    }
}
