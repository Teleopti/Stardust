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
		private readonly ITokenIdentityProvider _tokenIdentityProvider;

		public DataSourcesProvider(IApplicationData applicationData,
		                           IAvailableWindowsDataSources availableWindowsDataSources,
		                           ITokenIdentityProvider tokenIdentityProvider)
		{
			_applicationData = applicationData;
			_availableWindowsDataSources = availableWindowsDataSources;
			_tokenIdentityProvider = tokenIdentityProvider;
		}

		public IEnumerable<IDataSource> RetrieveDatasourcesForApplication()
		{
			return _applicationData.RegisteredDataSourceCollection;
		}

		public IEnumerable<IDataSource> RetrieveDatasourcesForWindows()
		{
			var winAccount = _tokenIdentityProvider.RetrieveToken();
			return winAccount == null
			       	? null
			       	: _availableWindowsDataSources.AvailableDataSources(_applicationData.RegisteredDataSourceCollection,
			       	                                                    winAccount.UserDomain,
			       	                                                    winAccount.UserIdentifier);
		}

		public IDataSource RetrieveDataSourceByName(string dataSourceName)
		{
			return _applicationData.RegisteredDataSourceCollection.FirstOrDefault(x => x.DataSourceName.Equals(dataSourceName));
		}
	}
}