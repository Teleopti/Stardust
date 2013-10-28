using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_quality_quest
    {
        public dim_quality_quest()
        {
            this.fact_quality = new List<fact_quality>();
        }

        public int quality_quest_id { get; set; }
        public Nullable<int> quality_quest_agg_id { get; set; }
        public Nullable<int> quality_quest_original_id { get; set; }
        public Nullable<float> quality_quest_score_weight { get; set; }
        public string quality_quest_name { get; set; }
        public string quality_quest_type_name { get; set; }
        public string log_object_name { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public virtual ICollection<fact_quality> fact_quality { get; set; }
    }
}
