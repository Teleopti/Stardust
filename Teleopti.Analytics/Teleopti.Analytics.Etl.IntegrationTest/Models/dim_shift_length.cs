using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class dim_shift_length
    {
        public int shift_length_id { get; set; }
        public int shift_length_m { get; set; }
        public Nullable<decimal> shift_length_h { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
    }
}
