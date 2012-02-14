using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckNullUser : ICheckNullUser
    {
        private readonly ICheckSuperUser _checkSuperUser;

        public CheckNullUser(ICheckSuperUser checkSuperUser)
        {
            _checkSuperUser = checkSuperUser;
        }

        public AuthenticationResult CheckLogOn(IUnitOfWork unitOfWork, IPerson person, string password)
        {
            if (person==null)
            {
                return new AuthenticationResult
                           {
                               HasMessage = true,
                               Message = UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword,
                               Successful = false
                           };
            }
            return _checkSuperUser.CheckLogOn(unitOfWork,person,password);
        }
    }
}