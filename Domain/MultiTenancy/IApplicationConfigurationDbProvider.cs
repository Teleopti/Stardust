namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IApplicationConfigurationDbProvider
	{
		ApplicationConfigurationDb GetAll();
		string GetServerValue(ServerConfigurationKey key);
		string GetTenantValue(TenantApplicationConfigKey key);
	}
}
