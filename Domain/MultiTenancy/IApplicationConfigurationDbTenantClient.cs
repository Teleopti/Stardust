namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IApplicationConfigurationDbTenantClient
	{
		ApplicationConfigurationDb GetAll();

		string GetServerValue(ServerConfigurationKey key);

		string GetTenantValue(TenantApplicationConfigKey key);
	}
}
