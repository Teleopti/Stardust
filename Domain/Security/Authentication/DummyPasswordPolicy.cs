using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class DummyPasswordPolicy : IPasswordPolicy
    {
        public TimeSpan InvalidAttemptWindow
        {
            get { return TimeSpan.FromMinutes(30); }
        }

        public int MaxAttemptCount
        {
            get { return 3; }
        }

        public bool CheckPasswordStrength(string password)
        {
            return true;
        }

        public int PasswordValidForDayCount
        {
            get { return 60; }
        }

        public int PasswordExpireWarningDayCount
        {
            get { return 5; }
        }
    }
}
