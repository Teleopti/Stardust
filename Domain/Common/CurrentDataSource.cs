using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentDataSource : ICurrentDataSource
	{
		private readonly ICurrentIdentity _currentIdentity;
		private readonly IConfigReader _configReader;
		private readonly ICurrentApplicationData _applicationData;
		private readonly Lazy<IDataSource> _rtaConfigurationDataSource;
 
		public static ICurrentDataSource Make()
		{
			return new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal()), null, null);
		}

		public CurrentDataSource(ICurrentIdentity currentIdentity, IConfigReader configReader, ICurrentApplicationData applicationData)
		{
			_currentIdentity = currentIdentity;
			_configReader = configReader;
			_applicationData = applicationData;

			_rtaConfigurationDataSource = new Lazy<IDataSource>(dataSourceFromRtaConfiguration);
		}

		public IDataSource Current()
		{
			var identity = _currentIdentity.Current();
			if (identity != null)
				return identity.DataSource;
			if (_configReader == null)
				return null;
			if (_applicationData == null)
				return null;
			return _rtaConfigurationDataSource.Value;
		}

		private IDataSource dataSourceFromRtaConfiguration()
		{
			var configString = new SqlConnectionStringBuilder(_configReader.ConnectionStrings["RtaApplication"].ConnectionString);
			IDataSource dataSource = null;
			_applicationData.Current().DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				var c = new SqlConnectionStringBuilder(tenant.Application.ConnectionString);
				if (c.DataSource == configString.DataSource &&
				    c.InitialCatalog == configString.InitialCatalog)
				{
					dataSource = tenant;
				}
			});

			return dataSource;
		}

		public string CurrentName()
		{
			return Current().DataSourceName;
		}

	}
}