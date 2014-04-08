using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class AvailableApplicationIdentityDataSource : IAvailableApplicationTokenDataSource
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public AvailableApplicationIdentityDataSource(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public bool IsDataSourceAvailable(IDataSource dataSource, string userIdentifier)
		{
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				return personRepository.TryFindBasicAuthenticatedPerson(userIdentifier) != null;
			}
		}
	}
}