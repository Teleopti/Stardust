using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationConfigurationDbProviderFake : IApplicationConfigurationDbProvider
	{
		private ApplicationConfigurationDb _fakeConfig;

		public ApplicationConfigurationDbProviderFake()
		{
			_fakeConfig = new ApplicationConfigurationDb();
		}

		public void LoadFakeData(ApplicationConfigurationDb fakeConfig)
		{
			_fakeConfig = fakeConfig;
		}

		public ApplicationConfigurationDb GetConfiguration()
		{
			return _fakeConfig;
		}

		public string GetServerValue(ServerConfigurationKey key) => 
			_fakeConfig.Server.TryGetValue(key.ToString(), out var value) ? value : null;

		public string GetTenantValue(TenantApplicationConfigKey key) => 
			_fakeConfig.Tenant.TryGetValue(key.ToString(), out var value) ? value : null;
	}
}
