using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Domain.Security
{
	public interface INhibConfigDecryption
	{
		DataSourceConfig DecryptConfig(DataSourceConfig dataSourceConfig);
	}
}