using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface ICheckPasswordChange
    {
        AuthenticationResult Check(IUserDetail userDetail);
    }
}