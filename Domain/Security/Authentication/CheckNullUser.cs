using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckNullUser : ICheckNullUser
    {
	    private readonly IFindUserDetail _findUserDetail;

        public CheckNullUser(IFindUserDetail findUserDetail)
        {
	        _findUserDetail = findUserDetail;
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
            return _findUserDetail.CheckLogOn(unitOfWork,person,password);
        }
    }
}