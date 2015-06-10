using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IDataSourceConfigurationProvider
	{
		DataSourceConfiguration ForTenant(Tenant tenant);
	}
}