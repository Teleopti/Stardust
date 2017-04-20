using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationEncryption : IDataSourceConfigurationEncryption
	{
		public DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig)
		{
			return new DataSourceConfiguration(
				Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2),
				Encryption.EncryptStringToBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2));
		}
	}
}