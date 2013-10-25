using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class Folder
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; }
        public string Description { get; set; }
        public Nullable<int> ParentFolderId { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public string Inherited { get; set; }
        public System.DateTime CreateDate { get; set; }
        public int OwnerId { get; set; }
        public System.DateTime LastModifiedDate { get; set; }
        public int ModifiedId { get; set; }
    }
}
