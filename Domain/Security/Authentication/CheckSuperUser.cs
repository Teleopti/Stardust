using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckSuperUser : ICheckSuperUser
    {
        private readonly IFindUserDetail _findUserDetail;
        private readonly ISystemUserSpecification _systemUserSpecification;
        private readonly ISystemUserPasswordSpecification _systemUserPasswordSpecification;

        public CheckSuperUser(IFindUserDetail findUserDetail, ISystemUserSpecification systemUserSpecification, ISystemUserPasswordSpecification systemUserPasswordSpecification)
        {
            _findUserDetail = findUserDetail;
            _systemUserSpecification = systemUserSpecification;
            _systemUserPasswordSpecification = systemUserPasswordSpecification;
        }

        public AuthenticationResult CheckLogOn(IUnitOfWork unitOfWork, IPerson person, string password)
        {
            if (_systemUserSpecification.IsSatisfiedBy(person) &&
                _systemUserPasswordSpecification.IsSatisfiedBy(password))
            {
                return new AuthenticationResult {Successful = true,Person = person};
            }
            return _findUserDetail.CheckLogOn(unitOfWork, person, password);
        }
    }
}