using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class report_control_collection
    {
        public System.Guid Id { get; set; }
        public System.Guid ControlId { get; set; }
        public System.Guid CollectionId { get; set; }
        public int control_collection_id { get; set; }
        public int collection_id { get; set; }
        public int print_order { get; set; }
        public int control_id { get; set; }
        public string default_value { get; set; }
        public string control_name_resource_key { get; set; }
        public string fill_proc_param { get; set; }
        public string param_name { get; set; }
        public Nullable<int> depend_of1 { get; set; }
        public Nullable<int> depend_of2 { get; set; }
        public Nullable<int> depend_of3 { get; set; }
        public Nullable<int> depend_of4 { get; set; }
        public Nullable<System.Guid> DependOf1 { get; set; }
        public Nullable<System.Guid> DependOf2 { get; set; }
        public Nullable<System.Guid> DependOf3 { get; set; }
        public Nullable<System.Guid> DependOf4 { get; set; }
        public virtual report_control report_control { get; set; }
    }
}
