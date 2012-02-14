using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckBruteForce : ICheckBruteForce
    {
        private readonly IPasswordPolicy _passwordPolicy;

        public CheckBruteForce(IPasswordPolicy passwordPolicy)
        {
            _passwordPolicy = passwordPolicy;
        }

        public AuthenticationResult Check(IUserDetail userDetail)
        {
            var result = new AuthenticationResult {Successful = false, Person = userDetail.Person};
            userDetail.RegisterInvalidAttempt(_passwordPolicy);
            if (userDetail.InvalidAttempts>=_passwordPolicy.MaxAttemptCount)
            {
                userDetail.Lock();
                result.Successful = false;
                result.HasMessage = true;
                result.Message = UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword; //Maybe other message here?
            }
            return result;
        }
    }
}