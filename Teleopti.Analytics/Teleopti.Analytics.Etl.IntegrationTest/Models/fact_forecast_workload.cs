using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class fact_forecast_workload
    {
        public int date_id { get; set; }
        public short interval_id { get; set; }
        public System.DateTime start_time { get; set; }
        public int workload_id { get; set; }
        public short scenario_id { get; set; }
        public System.DateTime end_time { get; set; }
        public Nullable<int> skill_id { get; set; }
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
        public Nullable<int> business_unit_id { get; set; }
        public Nullable<short> datasource_id { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public Nullable<System.DateTime> datasource_update_date { get; set; }
        public virtual dim_date dim_date { get; set; }
        public virtual dim_interval dim_interval { get; set; }
        public virtual dim_scenario dim_scenario { get; set; }
        public virtual dim_skill dim_skill { get; set; }
        public virtual dim_workload dim_workload { get; set; }
    }
}
