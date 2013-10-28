using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class aspnet_applications
    {
        public aspnet_applications()
        {
            this.aspnet_Membership = new List<aspnet_Membership>();
            this.aspnet_Users = new List<aspnet_Users>();
        }

        public string ApplicationName { get; set; }
        public string LoweredApplicationName { get; set; }
        public System.Guid ApplicationId { get; set; }
        public string Description { get; set; }
        public virtual ICollection<aspnet_Membership> aspnet_Membership { get; set; }
        public virtual ICollection<aspnet_Users> aspnet_Users { get; set; }
    }
}
