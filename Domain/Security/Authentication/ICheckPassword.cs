using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface ICheckPassword
    {
        AuthenticationResult CheckLogOn(IUserDetail userDetail, string password);
    }
}