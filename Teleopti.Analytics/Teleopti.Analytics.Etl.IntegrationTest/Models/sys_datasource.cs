using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class sys_datasource
    {
        public short datasource_id { get; set; }
        public string datasource_name { get; set; }
        public Nullable<int> log_object_id { get; set; }
        public string log_object_name { get; set; }
        public Nullable<int> datasource_database_id { get; set; }
        public string datasource_database_name { get; set; }
        public string datasource_type_name { get; set; }
        public Nullable<int> time_zone_id { get; set; }
        public bool inactive { get; set; }
        public Nullable<System.DateTime> insert_date { get; set; }
        public Nullable<System.DateTime> update_date { get; set; }
        public string source_id { get; set; }
        public bool @internal { get; set; }
    }
}
