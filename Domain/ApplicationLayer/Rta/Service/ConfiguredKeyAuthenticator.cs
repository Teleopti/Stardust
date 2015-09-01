using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ConfiguredKeyAuthenticator : IAuthenticator
	{
		private readonly string _authenticationKey;
		private readonly Lazy<string> _rtaConfigurationDataSourceName;

		public ConfiguredKeyAuthenticator(IConfigReader configReader, ICurrentApplicationData applicationData)
		{
			_authenticationKey = configReader.AppConfig("AuthenticationKey");
			if (string.IsNullOrEmpty(_authenticationKey))
				_authenticationKey = Rta.LegacyAuthenticationKey;
			_rtaConfigurationDataSourceName = new Lazy<string>(() =>
			{
				var dataSource = DataSourceFromRtaConfiguration(configReader, applicationData);
				if (dataSource == null)
					return null;
				return dataSource.DataSourceName;
			});
		}

		public string TenantForKey(string authenticationKey)
		{
			return authenticationKey == _authenticationKey ? _rtaConfigurationDataSourceName.Value : null;
		}



		public static IDataSource DataSourceFromRtaConfiguration(IConfigReader configReader, ICurrentApplicationData applicationData)
		{
			if (configReader == null)
				return null;
			if (applicationData == null)
				return null;

			var configString = new SqlConnectionStringBuilder(configReader.ConnectionString("RtaApplication"));
			IDataSource dataSource = null;
			applicationData.Current().DoOnAllTenants_AvoidUsingThis(tenant =>
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

	}
}