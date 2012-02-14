using System;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security
{
    public class PasswordPolicy : IPasswordPolicy
    {
        private readonly ILoadPasswordPolicyService _service;

        public PasswordPolicy(ILoadPasswordPolicyService service)
        {
            _service = service;
        }

      

        public TimeSpan InvalidAttemptWindow
        {
            get { return _service.LoadInvalidAttemptWindow(); }
        }

        public int MaxAttemptCount
        {
            get { return _service.LoadMaxAttemptCount(); }
        }

        public bool CheckPasswordStrength(string password)
        {
            return _service.LoadPasswordStrengthRules().All(r => r.VerifyPasswordStrength(password));
        }

        public int PasswordValidForDayCount
        {
            get { return _service.LoadPasswordValidForDayCount(); }
        }

        public int PasswordExpireWarningDayCount
        {
            get { return _service.LoadPasswordExpireWarningDayCount(); }
        }
    }
}
