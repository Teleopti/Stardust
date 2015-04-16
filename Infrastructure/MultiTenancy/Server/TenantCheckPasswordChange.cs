using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface ITenantCheckPasswordChange
	{
		AuthenticationResult Check(ApplicationLogonInfo userDetail);
	}
	public class TenantCheckPasswordChange : ITenantCheckPasswordChange
	{
		private readonly Func<IPasswordPolicy> _passwordPolicy;

		public TenantCheckPasswordChange(Func<IPasswordPolicy> passwordPolicy)
		{
			_passwordPolicy = passwordPolicy;
		}

		public AuthenticationResult Check(ApplicationLogonInfo userDetail)
		{
			var lastPasswordChange = userDetail.LastPasswordChange;
			var passwordValidForDayCount = _passwordPolicy().PasswordValidForDayCount;
			var maxDays = DateTime.MaxValue.Subtract(lastPasswordChange);
			var result = new AuthenticationResult { Successful = true };
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
										  (int)expirationDate.Subtract(utcNow).TotalDays);
			}
			return result;
		}
	}
}