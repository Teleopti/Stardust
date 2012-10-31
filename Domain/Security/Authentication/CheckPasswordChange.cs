using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckPasswordChange : ICheckPasswordChange
    {
        private readonly IPasswordPolicy _passwordPolicy;
	    private readonly IUtcNow _now;

	    public CheckPasswordChange(IPasswordPolicy passwordPolicy, IUtcNow now)
        {
	        _passwordPolicy = passwordPolicy;
	        _now = now;
        }

	    public AuthenticationResult Check(IUserDetail userDetail)
        {
        	var lastPasswordChange = userDetail.LastPasswordChange;
        	var passwordValidForDayCount = _passwordPolicy.PasswordValidForDayCount;
        	var maxDays = DateTime.MaxValue.Subtract(lastPasswordChange);
            var result = new AuthenticationResult{Successful = true, Person = userDetail.Person};

			DateTime expirationDate = DateTime.MaxValue;
			if (passwordValidForDayCount<maxDays.TotalDays)
			{
				expirationDate = lastPasswordChange.AddDays(passwordValidForDayCount);
			}

			if (expirationDate<=_now.UtcDateTime())
            {
                userDetail.Lock();
                result.Successful = false;
                result.HasMessage = true;
                result.Message = UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword;
                return result;
            }
            DateTime warningDate = expirationDate.AddDays(-_passwordPolicy.PasswordExpireWarningDayCount);
            if (warningDate <= _now.UtcDateTime())
            {
                result.HasMessage = true;
                result.Message = string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.LogOnWarningPasswordWillSoonExpire,
                                               (int) expirationDate.Subtract(_now.UtcDateTime()).TotalDays);
            }
            return result;
        }
    }
}