using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationConfigurationDbProvider : IApplicationConfigurationDbProvider
	{
		private readonly IServerConfigurationRepository _serverRepo;
		private readonly ICurrentTenant  _currentTenant;

		public ApplicationConfigurationDbProvider(IServerConfigurationRepository serverRepo, ICurrentTenant currentTenant)
		{
			_serverRepo = serverRepo;
			_currentTenant = currentTenant;
		}

		public ApplicationConfigurationDb GetAll()
		{
			var serverDbConfig = _serverRepo?.AllConfigurations();
			var serverConfig = new Dictionary<ServerConfigurationKey, string>();
			foreach (var sc in serverDbConfig)
			{
				if (Enum.TryParse<ServerConfigurationKey>(sc.Key, out var enumKey))
				{
					serverConfig.Add(enumKey, sc.Value);
				}
			}

			var tenantDbConfig = _currentTenant?.Current()?.ApplicationConfig;
			var tenantConfig = new Dictionary<TenantApplicationConfigKey, string>();
			if (tenantDbConfig != null)
			{
				foreach (var sc in tenantDbConfig)
				{
					if (Enum.TryParse<TenantApplicationConfigKey>(sc.Key, out var enumKey))
					{
						tenantConfig.Add(enumKey, sc.Value);
					}
				}
			}

			return new ApplicationConfigurationDb
			{
				Server = serverConfig,
				Tenant = tenantConfig
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
