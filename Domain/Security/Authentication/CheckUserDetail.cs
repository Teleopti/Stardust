using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckUserDetail : ICheckUserDetail
    {
        private readonly ICheckPassword _checkPassword;

        public CheckUserDetail(ICheckPassword checkPassword)
        {
            _checkPassword = checkPassword;
        }

        public AuthenticationResult CheckLogOn(IUserDetail userDetail, string password)
        {
            if (userDetail.IsLocked)
            {
                return new AuthenticationResult
                           {
                               HasMessage = true,
                               Person = userDetail.Person,
                               Successful = false,
                               Message = UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword
                           };
            }
            return _checkPassword.CheckLogOn(userDetail, password);
        }
    }
}