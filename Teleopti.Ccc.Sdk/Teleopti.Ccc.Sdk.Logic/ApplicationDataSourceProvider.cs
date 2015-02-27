using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic
{
    public class ApplicationDataSourceProvider : IApplicationDataSourceProvider
    {
        private readonly IAvailableDataSourcesProvider _availableDataSourcesProvider;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IFindApplicationUser _checkLogOn;

        public ApplicationDataSourceProvider(IAvailableDataSourcesProvider availableDataSourcesProvider, IRepositoryFactory repositoryFactory, IFindApplicationUser checkLogOn)
        {
            _availableDataSourcesProvider = availableDataSourcesProvider;
            _repositoryFactory = repositoryFactory;
            _checkLogOn = checkLogOn;
        }

        public IEnumerable<DataSourceContainer> DataSourceList()
        {
            IList<DataSourceContainer> dataSourceList = new List<DataSourceContainer>();
            foreach (IDataSource dataSource in _availableDataSourcesProvider.AvailableDataSources())
            {
                    dataSourceList.Add(new DataSourceContainer(dataSource,_repositoryFactory, _checkLogOn,AuthenticationTypeOption.Application));
            }
            return dataSourceList;
        }
    }
}