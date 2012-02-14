using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface ICheckUserDetail
    {
        AuthenticationResult CheckLogOn(IUserDetail userDetail, string password);
    }
}