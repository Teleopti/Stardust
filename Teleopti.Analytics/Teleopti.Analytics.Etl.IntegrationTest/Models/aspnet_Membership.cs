using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class aspnet_Membership
    {
        public System.Guid ApplicationId { get; set; }
        public System.Guid UserId { get; set; }
        public string Password { get; set; }
        public int PasswordFormat { get; set; }
        public string PasswordSalt { get; set; }
        public string MobilePIN { get; set; }
        public string Email { get; set; }
        public string LoweredEmail { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public System.DateTime CreateDate { get; set; }
        public System.DateTime LastLoginDate { get; set; }
        public System.DateTime LastPasswordChangedDate { get; set; }
        public System.DateTime LastLockoutDate { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public System.DateTime FailedPasswordAttemptWindowStart { get; set; }
        public int FailedPasswordAnswerAttemptCount { get; set; }
        public System.DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
        public string Comment { get; set; }
        public virtual aspnet_applications aspnet_applications { get; set; }
        public virtual aspnet_Users aspnet_Users { get; set; }
    }
}
