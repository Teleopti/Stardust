using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class PasswordVerifier : IPasswordVerifier
	{
		private readonly IOneWayEncryption _oneWayEncryption;
		private readonly Func<IPasswordPolicy> _passwordPolicy;
		private readonly INow _now;

		public PasswordVerifier(IOneWayEncryption oneWayEncryption, Func<IPasswordPolicy> passwordPolicy, INow now)
		{
			_oneWayEncryption = oneWayEncryption;
			_passwordPolicy = passwordPolicy;
			_now = now;
		}

		public bool Check(string userPassword, PasswordPolicyForUser passwordPolicyForUser)
		{
			var passwordPolicy = _passwordPolicy();
			var utcNow = _now.UtcDateTime();
			if (utcNow > passwordPolicyForUser.InvalidAttemptsSequenceStart.Add(passwordPolicy.InvalidAttemptWindow))
			{
				passwordPolicyForUser.ClearInvalidAttempts();
			}

			var isValid = passwordPolicyForUser.IsValidPassword(_oneWayEncryption.EncryptString(userPassword));
			if (!isValid)
			{
				if (passwordPolicyForUser.InvalidAttempts > passwordPolicy.MaxAttemptCount)
				{
					passwordPolicyForUser.Lock();
				}
			}
			return isValid;
		}
	}
}