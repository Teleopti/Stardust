using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_request_status
    {
        public dim_request_status()
        {
            this.fact_request = new List<fact_request>();
            this.fact_requested_days = new List<fact_requested_days>();
        }

        public byte request_status_id { get; set; }
        public string request_status_name { get; set; }
        public string resource_key { get; set; }
        public virtual ICollection<fact_request> fact_request { get; set; }
        public virtual ICollection<fact_requested_days> fact_requested_days { get; set; }
    }
}
