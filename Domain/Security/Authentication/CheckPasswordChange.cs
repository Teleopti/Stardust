using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckPasswordChange : ICheckPasswordChange
    {
        private readonly IPasswordPolicy _passwordPolicy;

	    public CheckPasswordChange(IPasswordPolicy passwordPolicy)
	    {
		    _passwordPolicy = passwordPolicy;
	    }

	    public AuthenticationResult Check(IUserDetail userDetail)
        {
        	var lastPasswordChange = userDetail.LastPasswordChange;
        	var passwordValidForDayCount = _passwordPolicy.PasswordValidForDayCount;
        	var maxDays = DateTime.MaxValue.Subtract(lastPasswordChange);
            var result = new AuthenticationResult{Successful = true, Person = userDetail.Person};
		    var utcNow = DateTime.UtcNow;

			var expirationDate = DateTime.MaxValue;
			if (passwordValidForDayCount<maxDays.TotalDays)
			{
				expirationDate = lastPasswordChange.AddDays(passwordValidForDayCount);
			}

			if (expirationDate <= utcNow)
            {
                result.Successful = false;
                result.HasMessage = true;
				result.Message = UserTexts.Resources.LogOnFailedPasswordExpired;
                return result;
            }
            var warningDate = expirationDate.AddDays(-_passwordPolicy.PasswordExpireWarningDayCount);
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