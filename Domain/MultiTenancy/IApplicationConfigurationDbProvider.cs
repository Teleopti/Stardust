namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IApplicationConfigurationDbProvider
	{
		ApplicationConfigurationDb GetConfiguration();
		string GetServerValue(ServerConfigurationKey key);
		string GetTenantValue(TenantApplicationConfigKey key);
	}
}
