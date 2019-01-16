namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IApplicationConfigurationDbProvider
	{
		ApplicationConfigurationDb GetConfiguration();
		string TryGetServerValue(ServerConfigurationKey key, string defaultValue = "");
		string TryGetTenantValue(TenantApplicationConfigKey key, string defaultValue = "");
	}
}
