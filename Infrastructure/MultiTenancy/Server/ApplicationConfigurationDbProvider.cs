using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationConfigurationDbProvider : IApplicationConfigurationDbProvider
	{
		private readonly IServerConfigurationRepository _serverRepo;
		private readonly ICurrentTenant _currentTenant;

		public ApplicationConfigurationDbProvider(IServerConfigurationRepository serverRepo, ICurrentTenant currentTenant)
		{
			_serverRepo = serverRepo;
			_currentTenant = currentTenant;
		}

		public ApplicationConfigurationDb GetConfiguration()
		{
			return new ApplicationConfigurationDb
			{
				Server = _serverRepo?.AllConfigurations()?.ToDictionary(k => k.Key, v => v.Value),
				Tenant = _currentTenant?.Current()?.ApplicationConfig?.ToDictionary(k => k.Key, v => v.Value)
			};
		}

		public string TryGetServerValue(ServerConfigurationKey key, string defaultValue = "")
		{
			return _serverRepo?.Get(key.ToString()) ?? defaultValue;
		}

		public string TryGetTenantValue(TenantApplicationConfigKey key, string defaultValue = "")
		{
			string value = string.Empty;
			if (_currentTenant?.Current()?.ApplicationConfig?.TryGetValue(key.ToString(), out value) ?? false)
			{
				return value;
			}
			return defaultValue;
		}
	}
}
