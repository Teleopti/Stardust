using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class custom_report_control_collection
    {
        public System.Guid Id { get; set; }
        public System.Guid CollectionId { get; set; }
        public System.Guid ControlId { get; set; }
        public int print_order { get; set; }
        public string default_value { get; set; }
        public string control_name_resource_key { get; set; }
        public string fill_proc_param { get; set; }
        public string param_name { get; set; }
        public Nullable<System.Guid> DependOf1 { get; set; }
        public Nullable<System.Guid> DependOf2 { get; set; }
        public Nullable<System.Guid> DependOf3 { get; set; }
        public Nullable<System.Guid> DependOf4 { get; set; }
        public virtual custom_report_control custom_report_control { get; set; }
    }
}
