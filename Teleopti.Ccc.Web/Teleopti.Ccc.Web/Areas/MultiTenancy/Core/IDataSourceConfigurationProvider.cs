namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IDataSourceConfigurationProvider
	{
		DataSourceConfiguration ForTenant(string tenant);
	}
}