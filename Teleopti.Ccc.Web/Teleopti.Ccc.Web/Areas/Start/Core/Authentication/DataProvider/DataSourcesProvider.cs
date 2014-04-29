using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class DataSourcesProvider : IDataSourcesProvider
	{
		private readonly IApplicationData _applicationData;
		private readonly IAvailableIdentityDataSources _availableIdentityDataSources;
		private readonly IAvailableApplicationTokenDataSource _availableApplicationTokenDataSource;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;

		public DataSourcesProvider(IApplicationData applicationData, IAvailableIdentityDataSources _availableIdentityDataSources, IAvailableApplicationTokenDataSource availableApplicationTokenDataSource, ITokenIdentityProvider tokenIdentityProvider)
		{
			_applicationData = applicationData;
			this._availableIdentityDataSources = _availableIdentityDataSources;
			_availableApplicationTokenDataSource = availableApplicationTokenDataSource;
			_tokenIdentityProvider = tokenIdentityProvider;
		}

		public IEnumerable<IDataSource> RetrieveDatasourcesForApplication()
		{
			return _applicationData.RegisteredDataSourceCollection;
		}

		public IEnumerable<IDataSource> RetrieveDatasourcesForIdentity()
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			return token == null
				? null
				: _availableIdentityDataSources.AvailableDataSources(_applicationData.RegisteredDataSourceCollection,
					token.OriginalToken);
		}

		public IDataSource RetrieveDataSourceByName(string dataSourceName)
		{
			return _applicationData.RegisteredDataSourceCollection.FirstOrDefault(x => x.DataSourceName.Equals(dataSourceName));
		}

		public IEnumerable<IDataSource> RetrieveDatasourcesForApplicationIdentityToken()
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			var dataSource = RetrieveDataSourceByName(token.DataSource);

			return ( dataSource != null && _availableApplicationTokenDataSource.IsDataSourceAvailable(dataSource, token.UserIdentifier))
				? new[] {dataSource}
				: null;
		}
	}
}