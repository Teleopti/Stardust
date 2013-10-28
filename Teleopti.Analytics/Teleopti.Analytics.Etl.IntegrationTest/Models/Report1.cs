using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class Report1
    {
        public byte[] Definition { get; set; }
        public int ReportId { get; set; }
        public string ReportName { get; set; }
        public string Description { get; set; }
        public int FolderId { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public string Inherited { get; set; }
        public System.DateTime CreateDate { get; set; }
        public int OwnerId { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public int ModifiedId { get; set; }
    }
}
