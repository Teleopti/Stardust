namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IDataSourceConfigurationEncryption
	{
		DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig);
	}
}