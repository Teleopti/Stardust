using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class FindApplicationUser : IFindApplicationUser
    {
        private readonly ICheckNullUser _checkNullUser;
        private readonly IRepositoryFactory _repositoryFactory;

        public FindApplicationUser(ICheckNullUser checkNullUser, IRepositoryFactory repositoryFactory)
        {
            _checkNullUser = checkNullUser;
            _repositoryFactory = repositoryFactory;
        }

        public AuthenticationResult CheckLogOn(IUnitOfWork unitOfWork, string logOnName, string password)
        {
            IPersonRepository personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
            IPerson user = personRepository.TryFindBasicAuthenticatedPerson(logOnName);
            return _checkNullUser.CheckLogOn(unitOfWork, user, password);
        }
    }
}