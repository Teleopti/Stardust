using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface IFindApplicationUser
    {
        AuthenticationResult CheckLogOn(IUnitOfWork unitOfWork, string logOnName, string password);
    }
}