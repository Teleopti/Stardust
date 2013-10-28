using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_request
    {
        public System.Guid request_code { get; set; }
        public System.Guid person_code { get; set; }
        public System.DateTime application_datetime { get; set; }
        public System.DateTime request_date { get; set; }
        public System.DateTime request_startdate { get; set; }
        public System.DateTime request_enddate { get; set; }
        public byte request_type_code { get; set; }
        public byte request_status_code { get; set; }
        public int request_start_date_count { get; set; }
        public int request_day_count { get; set; }
        public System.Guid business_unit_code { get; set; }
        public short datasource_id { get; set; }
        public System.DateTime insert_date { get; set; }
        public System.DateTime update_date { get; set; }
        public System.DateTime datasource_update_date { get; set; }
        public short is_deleted { get; set; }
        public System.DateTime request_starttime { get; set; }
        public System.DateTime request_endtime { get; set; }
        public Nullable<System.Guid> absence_code { get; set; }
    }
}
