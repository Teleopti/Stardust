using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class PasswordVerifier : IPasswordVerifier
	{
		private readonly Func<IPasswordPolicy> _passwordPolicy;
		private readonly INow _now;

		public PasswordVerifier(Func<IPasswordPolicy> passwordPolicy, INow now)
		{
			_passwordPolicy = passwordPolicy;
			_now = now;
		}

		public bool Check(string userPassword, ApplicationLogonInfo applicationLogonInfo)
		{
			var passwordPolicy = _passwordPolicy();
			var utcNow = _now.UtcDateTime();
			if (utcNow > applicationLogonInfo.InvalidAttemptsSequenceStart.Add(passwordPolicy.InvalidAttemptWindow))
			{
				applicationLogonInfo.ClearInvalidAttempts();
			}

			var isValid = applicationLogonInfo.IsValidPassword(userPassword);
			if (!isValid)
			{
				if (applicationLogonInfo.InvalidAttempts > passwordPolicy.MaxAttemptCount)
				{
					applicationLogonInfo.Lock();
				}
			}
			return isValid;
		}
	}
}