using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IDataSourceConfigurationEncryption
	{
		DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig);
	}
}