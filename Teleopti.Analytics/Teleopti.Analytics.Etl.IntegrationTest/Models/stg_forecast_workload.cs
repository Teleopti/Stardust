using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_forecast_workload
    {
        public System.DateTime date { get; set; }
        public short interval_id { get; set; }
        public System.DateTime start_time { get; set; }
        public System.Guid workload_code { get; set; }
        public System.Guid scenario_code { get; set; }
        public System.DateTime end_time { get; set; }
        public Nullable<System.Guid> skill_code { get; set; }
        public Nullable<decimal> forecasted_calls { get; set; }
        public Nullable<decimal> forecasted_emails { get; set; }
        public Nullable<decimal> forecasted_backoffice_tasks { get; set; }
        public Nullable<decimal> forecasted_campaign_calls { get; set; }
        public Nullable<decimal> forecasted_calls_excl_campaign { get; set; }
        public Nullable<decimal> forecasted_talk_time_s { get; set; }
        public Nullable<decimal> forecasted_campaign_talk_time_s { get; set; }
        public Nullable<decimal> forecasted_talk_time_excl_campaign_s { get; set; }
        public Nullable<decimal> forecasted_after_call_work_s { get; set; }
        public Nullable<decimal> forecasted_campaign_after_call_work_s { get; set; }
        public Nullable<decimal> forecasted_after_call_work_excl_campaign_s { get; set; }
        public Nullable<decimal> forecasted_handling_time_s { get; set; }
        public Nullable<decimal> forecasted_campaign_handling_time_s { get; set; }
        public Nullable<decimal> forecasted_handling_time_excl_campaign_s { get; set; }
        public Nullable<decimal> period_length_min { get; set; }
        public Nullable<System.Guid> business_unit_code { get; set; }
        public string business_unit_name { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
    }
}
