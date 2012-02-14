using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class FindUserDetail : IFindUserDetail
    {
        private readonly ICheckUserDetail _checkUserDetail;
        private readonly IRepositoryFactory _repositoryFactory;

        public FindUserDetail(ICheckUserDetail checkUserDetail, IRepositoryFactory repositoryFactory)
        {
            _checkUserDetail = checkUserDetail;
            _repositoryFactory = repositoryFactory;
        }

        public AuthenticationResult CheckLogOn(IUnitOfWork unitOfWork, IPerson person, string password)
        {
            var repository = _repositoryFactory.CreateUserDetailRepository(unitOfWork);
            IUserDetail userDetail = repository.FindByUser(person);
            return _checkUserDetail.CheckLogOn(userDetail, password);
        }
    }
}