using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
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
				if ((availableDataSource.AuthenticationTypeOption & AuthenticationTypeOption.Windows) !=
					AuthenticationTypeOption.Windows) continue;

                bool availableForUser;
                IPerson person;
                using (var unitOfWork = availableDataSource.Application.CreateAndOpenUnitOfWork())
                {
                    var personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
                    availableForUser =
                        personRepository.TryFindIdentityAuthenticatedPerson(_windowsUserProvider.DomainName, out person);
                }
                if (availableForUser)
                {
                    var container = new DataSourceContainer(availableDataSource, _repositoryFactory, null,
                                                            AuthenticationTypeOption.Windows);
					container.LogOnName = _windowsUserProvider.DomainName + "\\" + _windowsUserProvider.UserName;
                    container.SetUser(person);
                    dataSourceList.Add(container);
                }
            }
            return dataSourceList;
        }
    }
}