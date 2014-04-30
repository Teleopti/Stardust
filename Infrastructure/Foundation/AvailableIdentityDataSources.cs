using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class AvailableIdentityDataSources : IAvailableIdentityDataSources
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public AvailableIdentityDataSources(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public IEnumerable<IDataSource> AvailableDataSources(IEnumerable<IDataSource> dataSourcesToScan, string identity)
		{
			var retList = new List<IDataSource>();
			foreach (var dataSource in dataSourcesToScan)
			{
				tryAddDataSourceToReturnList(dataSource, identity, retList);
			}
			return retList;
		}

		private void tryAddDataSourceToReturnList(IDataSource dataSource, string identity, ICollection<IDataSource> retList)
		{
			using(var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				if (personRep.DoesIdentityExists(identity))
				{
					retList.Add(dataSource);
				}
			}
		}
	}
}