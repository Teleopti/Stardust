using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class VerifyPasswordPolicy : IVerifyPasswordPolicy
	{
		private readonly Func<IPasswordPolicy> _passwordPolicy;

		public VerifyPasswordPolicy(Func<IPasswordPolicy> passwordPolicy)
		{
			_passwordPolicy = passwordPolicy;
		}

		public PasswordPolicyResult Check(ApplicationLogonInfo userDetail)
		{
			var lastPasswordChange = userDetail.LastPasswordChange;
			var passwordValidForDayCount = _passwordPolicy().PasswordValidForDayCount;
			var maxDays = DateTime.MaxValue.Subtract(lastPasswordChange);
			var result = new PasswordPolicyResult { Successful = true };
			var utcNow = DateTime.UtcNow;

			var expirationDate = DateTime.MaxValue;
			if (passwordValidForDayCount < maxDays.TotalDays)
			{
				expirationDate = lastPasswordChange.AddDays(passwordValidForDayCount);
			}

			if (expirationDate <= utcNow)
			{
				result.Successful = false;
				result.HasMessage = true;
				result.PasswordExpired = true;
				result.Message = UserTexts.Resources.LogOnFailedPasswordExpired;
				return result;
			}
			var warningDate = expirationDate.AddDays(-_passwordPolicy().PasswordExpireWarningDayCount);
			if (warningDate <= utcNow)
			{
				result.HasMessage = true;
				result.Message = string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.LogOnWarningPasswordWillSoonExpire,
										  Math.Ceiling(expirationDate.Subtract(utcNow).TotalDays));
			}
			return result;
		}
	}
}