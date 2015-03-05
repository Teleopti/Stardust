using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Domain.Security
{
	public interface INhibConfigEncryption
	{
		DataSourceConfig EncryptConfig(DataSourceConfig dataSourceConfig);
		void DecryptConfig(DataSourceConfig dataSourceConfig);
	}
}