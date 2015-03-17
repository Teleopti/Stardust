namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface INhibConfigurationEncryption
	{
		DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig);
	}
}