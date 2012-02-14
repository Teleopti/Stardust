using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class DataSourcesProvider : IDataSourcesProvider
	{
		private readonly IApplicationData _applicationData;
		private readonly IAvailableWindowsDataSources _availableWindowsDataSources;
		private readonly IWindowsAccountProvider _windowsAccountProvider;

		public DataSourcesProvider(IApplicationData applicationData,
		                           IAvailableWindowsDataSources availableWindowsDataSources,
		                           IWindowsAccountProvider windowsAccountProvider)
		{
			_applicationData = applicationData;
			_availableWindowsDataSources = availableWindowsDataSources;
			_windowsAccountProvider = windowsAccountProvider;
		}

		public IEnumerable<IDataSource> RetrieveDatasourcesForApplication()
		{
			return _applicationData.RegisteredDataSourceCollection;
		}

		public IEnumerable<IDataSource> RetrieveDatasourcesForWindows()
		{
			var winAccount = _windowsAccountProvider.RetrieveWindowsAccount();
			return winAccount == null
			       	? null
			       	: _availableWindowsDataSources.AvailableDataSources(_applicationData.RegisteredDataSourceCollection,
			       	                                                    winAccount.DomainName,
			       	                                                    winAccount.UserName);
		}

		public IDataSource RetrieveDataSourceByName(string dataSourceName)
		{
			return _applicationData.RegisteredDataSourceCollection.Where(x => x.DataSourceName.Equals(dataSourceName)).First();
		}
	}
}