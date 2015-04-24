namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface INhibConfigurationEncryption
	{
		DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig);
	}
}