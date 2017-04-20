using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Domain.Security
{
	public class DataSourceConfigDecryption : IDataSourceConfigDecryption
	{
		public DataSourceConfig EncryptConfigJustForTest(DataSourceConfig dataSourceConfig)
		{
			return new DataSourceConfig
			{
				ApplicationConnectionString =
					Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1,
						EncryptionConstants.Image2),
				AnalyticsConnectionString =
					Encryption.EncryptStringToBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1,
						EncryptionConstants.Image2)
			};
		}

		public DataSourceConfig DecryptConfig(DataSourceConfig dataSourceConfig)
		{
			return new DataSourceConfig
			{
				AnalyticsConnectionString = Encryption.DecryptStringFromBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2),
				ApplicationConnectionString = Encryption.DecryptStringFromBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2)
			};
		}
	}
}