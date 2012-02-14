using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface ICheckBruteForce
    {
        AuthenticationResult Check(IUserDetail userDetail);
    }
}