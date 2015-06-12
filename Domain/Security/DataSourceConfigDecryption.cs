using System.Linq;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security
{
	public class DataSourceConfigDecryption : IDataSourceConfigDecryption
	{
		public DataSourceConfig EncryptConfigJustForTest(DataSourceConfig dataSourceConfig)
		{
			var dic = dataSourceConfig.ApplicationNHibernateConfig.Keys.ToDictionary(key => key, key => Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationNHibernateConfig[key], EncryptionConstants.Image1, EncryptionConstants.Image2));
			dataSourceConfig.ApplicationNHibernateConfig = dic;
			dataSourceConfig.AnalyticsConnectionString = Encryption.EncryptStringToBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);
			dataSourceConfig.ApplicationConnectionString = Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);
			return dataSourceConfig;
		}

		public DataSourceConfig DecryptConfig(DataSourceConfig dataSourceConfig)
		{
			return new DataSourceConfig
			{
				AnalyticsConnectionString = Encryption.DecryptStringFromBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2),
				ApplicationNHibernateConfig = dataSourceConfig.ApplicationNHibernateConfig.DecryptDictionary(EncryptionConstants.Image1, EncryptionConstants.Image2),
				ApplicationConnectionString = Encryption.DecryptStringFromBase64(dataSourceConfig.ApplicationConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2)
			};
		}
	}
}