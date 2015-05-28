using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IDataSourceConfigurationProvider
	{
		DataSourceConfiguration ForTenant(Tenant tenant);
	}
}