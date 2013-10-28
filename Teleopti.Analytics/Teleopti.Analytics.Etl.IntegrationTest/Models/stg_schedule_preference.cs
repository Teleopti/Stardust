using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_schedule_preference
    {
        public System.Guid person_restriction_code { get; set; }
        public System.DateTime restriction_date { get; set; }
        public System.Guid person_code { get; set; }
        public System.Guid scenario_code { get; set; }
        public Nullable<System.Guid> shift_category_code { get; set; }
        public Nullable<System.Guid> day_off_code { get; set; }
        public string day_off_name { get; set; }
        public string day_off_shortname { get; set; }
        public Nullable<long> StartTimeMinimum { get; set; }
        public Nullable<long> StartTimeMaximum { get; set; }
        public Nullable<long> endTimeMinimum { get; set; }
        public Nullable<long> endTimeMaximum { get; set; }
        public Nullable<long> WorkTimeMinimum { get; set; }
        public Nullable<long> WorkTimeMaximum { get; set; }
        public Nullable<int> preference_fulfilled { get; set; }
        public Nullable<int> preference_unfulfilled { get; set; }
        public System.Guid business_unit_code { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public Nullable<System.Guid> activity_code { get; set; }
        public Nullable<System.Guid> absence_code { get; set; }
        public Nullable<int> must_have { get; set; }
    }
}
