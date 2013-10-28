using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class stg_date
    {
        public System.DateTime date_date { get; set; }
        public int year { get; set; }
        public int year_month { get; set; }
        public int month { get; set; }
        public string month_name { get; set; }
        public string month_resource_key { get; set; }
        public int day_in_month { get; set; }
        public int weekday_number { get; set; }
        public string weekday_name { get; set; }
        public string weekday_resource_key { get; set; }
        public int week_number { get; set; }
        public string year_week { get; set; }
        public string quarter { get; set; }
        public System.DateTime insert_date { get; set; }
    }
}
