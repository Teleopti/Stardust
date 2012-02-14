using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface IFindUserDetail
    {
        AuthenticationResult CheckLogOn(IUnitOfWork unitOfWork, IPerson person, string password);
    }
}