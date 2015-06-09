using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class PasswordPolicyFake : IPasswordPolicy
	{
		private bool _valid = true;
		public PasswordPolicyFake()
		{
			InvalidAttemptWindow = TimeSpan.FromMinutes(30);
			MaxAttemptCount = 3;
			PasswordValidForDayCount = 60;
			PasswordExpireWarningDayCount = 5;
		}

		public int PasswordExpireWarningDayCount { get; set; }
		public int PasswordValidForDayCount { get; set; }
		public int MaxAttemptCount { get; set; }
		public TimeSpan InvalidAttemptWindow { get; set; }

		public bool CheckPasswordStrength(string password)
		{
			return _valid;
		}

		public void SetInvalid()
		{
			_valid = false;
		}
	}
}
