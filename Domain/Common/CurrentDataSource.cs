using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentDataSource : ICurrentDataSource
	{
		private readonly ICurrentIdentity _currentIdentity;
		private readonly IConfigReader _configReader;
		private readonly IAvailableDataSourcesProvider _dataSourcesProvider;
		private readonly Lazy<IDataSource> _rtaConfigurationDataSource;
 
		public static ICurrentDataSource Make()
		{
			return new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal()), null, null);
		}

		public CurrentDataSource(ICurrentIdentity currentIdentity, IConfigReader configReader, IAvailableDataSourcesProvider dataSourcesProvider)
		{
			_currentIdentity = currentIdentity;
			_configReader = configReader;
			_dataSourcesProvider = dataSourcesProvider;
			_rtaConfigurationDataSource = new Lazy<IDataSource>(dataSourceFromRtaConfiguration);
		}

		public IDataSource Current()
		{
			var identity = _currentIdentity.Current();
			if (identity != null)
				return identity.DataSource;
			if (_configReader == null)
				return null;
			if (_dataSourcesProvider == null)
				return null;
			return _rtaConfigurationDataSource.Value;
		}

		private IDataSource dataSourceFromRtaConfiguration()
		{
			var configString = new SqlConnectionStringBuilder(_configReader.ConnectionStrings["RtaApplication"].ConnectionString);
			var matching =
				from ds in _dataSourcesProvider.AvailableDataSources()
				let c = new SqlConnectionStringBuilder(ds.Application.ConnectionString)
				where c.DataSource == configString.DataSource &&
				      c.InitialCatalog == configString.InitialCatalog
				select ds;

			return matching.Single();
		}

		public string CurrentName()
		{
			return Current().DataSourceName;
		}

	}
}