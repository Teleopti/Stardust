using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Domain.Security
{
	public interface IDataSourceConfigDecryption
	{
		DataSourceConfig DecryptConfig(DataSourceConfig dataSourceConfig);
	}
}