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

		public string GetServerValue(ServerConfigurationKey key)
		{
			return _serverRepo?.Get(key);
		}

		public string GetTenantValue(TenantApplicationConfigKey key)
		{
			return _currentTenant?.Current()?.GetApplicationConfig(key);
		}
	}
}
