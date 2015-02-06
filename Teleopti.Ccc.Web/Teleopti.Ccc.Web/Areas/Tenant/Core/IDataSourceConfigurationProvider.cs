namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IDataSourceConfigurationProvider
	{
		DataSourceConfiguration ForTenant(string tenant);
	}
}