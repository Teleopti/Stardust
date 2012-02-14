using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class AvailableWindowsDataSources : IAvailableWindowsDataSources
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public AvailableWindowsDataSources(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public IEnumerable<IDataSource> AvailableDataSources(IEnumerable<IDataSource> dataSourcesToScan,
																string domainName,
																string userName)
		{
			var retList = new List<IDataSource>();
			foreach (var dataSource in dataSourcesToScan)
			{
				tryAddDataSourceToReturnList(dataSource, domainName, userName, retList);
			}
			return retList;
		}

		private void tryAddDataSourceToReturnList(IDataSource dataSource, string domainName, string userName, ICollection<IDataSource> retList)
		{
			using(var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				if(personRep.DoesWindowsUserExists(domainName, userName))
				{
					retList.Add(dataSource);
				}
			}
		}
	}
}