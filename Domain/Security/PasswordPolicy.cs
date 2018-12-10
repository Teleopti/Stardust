using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Security
{
    public class PasswordPolicy : IPasswordPolicy
    {
        private readonly ILoadPasswordPolicyService _service;

        public PasswordPolicy(ILoadPasswordPolicyService service)
        {
            _service = service;
        }
		
        public TimeSpan InvalidAttemptWindow => _service.LoadInvalidAttemptWindow();

		public int MaxAttemptCount => _service.LoadMaxAttemptCount();

		public bool CheckPasswordStrength(string password)
        {
            return _service.LoadPasswordStrengthRules().All(r => r.VerifyPasswordStrength(password));
        }

        public int PasswordValidForDayCount => _service.LoadPasswordValidForDayCount();

		public int PasswordExpireWarningDayCount => _service.LoadPasswordExpireWarningDayCount();
	}
}
