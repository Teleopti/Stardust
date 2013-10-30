using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class report_user_setting
    {
        public System.Guid ReportId { get; set; }
        public System.Guid person_code { get; set; }
        public string param_name { get; set; }
        public int saved_name_id { get; set; }
        public string control_setting { get; set; }
    }
}
