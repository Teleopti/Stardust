using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic
{
	public class WindowsDataSourceProvider : IDataSourceProvider
	{
		private readonly IAvailableDataSourcesProvider _availableDataSourcesProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IWindowsUserProvider _windowsUserProvider;

		public WindowsDataSourceProvider(IAvailableDataSourcesProvider availableDataSourcesProvider, IRepositoryFactory repositoryFactory, IWindowsUserProvider windowsUserProvider)
		{
			_availableDataSourcesProvider = availableDataSourcesProvider;
			_repositoryFactory = repositoryFactory;
			_windowsUserProvider = windowsUserProvider;
		}

		public IEnumerable<DataSourceContainer> DataSourceList()
		{
			var dataSourceList = new List<DataSourceContainer>();
			var availableDataSources = _availableDataSourcesProvider.AvailableDataSources();
			foreach (IDataSource availableDataSource in availableDataSources)
			{
				bool availableForUser;
				IPerson person;
				var domainName = _windowsUserProvider.DomainName;
				var userName = _windowsUserProvider.UserName;
				var logOnName = IdentityHelper.Merge(domainName, userName);
				using (var unitOfWork = availableDataSource.Application.CreateAndOpenUnitOfWork())
				{
					var personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
					availableForUser =
				 personRepository.TryFindIdentityAuthenticatedPerson(logOnName, out person);
				}
				if (availableForUser)
				{
					var container = new DataSourceContainer(availableDataSource, _repositoryFactory, null,
																		 AuthenticationTypeOption.Windows);
					container.LogOnName = logOnName;
					container.SetUser(person);
					dataSourceList.Add(container);
				}
			}
			return dataSourceList;
		}
	}
}