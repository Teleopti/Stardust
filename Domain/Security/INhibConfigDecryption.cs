using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Domain.Security
{
	public interface INhibConfigDecryption
	{
		void DecryptConfig(DataSourceConfig dataSourceConfig);
	}
}