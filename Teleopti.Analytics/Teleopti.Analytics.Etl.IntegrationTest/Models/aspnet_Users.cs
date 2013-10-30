using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class aspnet_Users
    {
        public System.Guid ApplicationId { get; set; }
        public System.Guid UserId { get; set; }
        public string UserName { get; set; }
        public string LoweredUserName { get; set; }
        public string MobileAlias { get; set; }
        public bool IsAnonymous { get; set; }
        public System.DateTime LastActivityDate { get; set; }
        public string AppLoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int LanguageId { get; set; }
        public int CultureId { get; set; }
        public virtual aspnet_applications aspnet_applications { get; set; }
        public virtual aspnet_Membership aspnet_Membership { get; set; }
    }
}
