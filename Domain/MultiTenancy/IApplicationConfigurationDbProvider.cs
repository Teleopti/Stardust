namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IApplicationConfigurationDbProvider
	{
		ApplicationConfigurationDb GetConfiguration();
		string TryGetServerValue(string key, string defaultValue = "");
		string TryGetTenantValue(string key, string defaultValue = "");
	}
}
