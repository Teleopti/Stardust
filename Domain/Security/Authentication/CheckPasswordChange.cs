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
            var result = new AuthenticationResult{Successful = true, Person = userDetail.Person};
            DateTime expirationDate;
            try
            {
                 expirationDate = userDetail.LastPasswordChange.AddDays(_passwordPolicy.PasswordValidForDayCount);

            }
            catch(ArgumentException)
            {
                expirationDate = DateTime.MaxValue;
            }

            if (expirationDate<=DateTime.UtcNow)
            {
                userDetail.Lock();
                result.Successful = false;
                result.HasMessage = true;
                result.Message = UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword;
                return result;
            }
            DateTime warningDate = expirationDate.AddDays(-_passwordPolicy.PasswordExpireWarningDayCount);
            if (warningDate <= DateTime.UtcNow)
            {
                result.HasMessage = true;
                result.Message = string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.LogOnWarningPasswordWillSoonExpire,
                                               (int) expirationDate.Subtract(DateTime.UtcNow).TotalDays);
            }
            return result;
        }
    }
}