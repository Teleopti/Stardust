using System.Collections.Specialized;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class ApplicationConfigurationDbTenantClient : IApplicationConfigurationDbTenantClient
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IGetHttpRequest _getHttpRequest;
		private readonly ICurrentTenantCredentials _currentTenantCredentials;

		public ApplicationConfigurationDbTenantClient(ITenantServerConfiguration tenantServerConfiguration, 
			IGetHttpRequest getHttpRequest,
			ICurrentTenantCredentials currentTenantCredentials)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_getHttpRequest = getHttpRequest;
			_currentTenantCredentials = currentTenantCredentials;
		}

		public ApplicationConfigurationDb GetAll()
		{
			var tenantCredentials = _currentTenantCredentials.TenantCredentials();
			return _getHttpRequest.GetSecured<ApplicationConfigurationDb>(
				_tenantServerConfiguration.FullPath("Configuration/GetAll"), new NameValueCollection(),
				tenantCredentials);
		}

		public string GetServerValue(ServerConfigurationKey key)
		{
			var tenantCredentials = _currentTenantCredentials.TenantCredentials();
			return _getHttpRequest.GetSecured<string>(_tenantServerConfiguration.FullPath("Configuration/GetServerValue"),
				new NameValueCollection {{"key", key.ToString()}}, tenantCredentials);
		}

		public string GetTenantValue(TenantApplicationConfigKey key)
		{
			var tenantCredentials = _currentTenantCredentials.TenantCredentials();
			return _getHttpRequest.GetSecured<string>(_tenantServerConfiguration.FullPath("Configuration/GetTenantValue"),
				new NameValueCollection {{"key", key.ToString()}}, tenantCredentials);
		}
	}
}