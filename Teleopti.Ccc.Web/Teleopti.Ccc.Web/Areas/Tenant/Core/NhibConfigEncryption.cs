using System.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class NhibConfigurationEncryption : INhibConfigurationEncryption
	{
		public DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig)
		{
			var dic = dataSourceConfig.ApplicationNHibernateConfig.Keys.ToDictionary(key => key, key => Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationNHibernateConfig[key], EncryptionConstants.Image1, EncryptionConstants.Image2));
			var ret = new DataSourceConfiguration
			{
				ApplicationNHibernateConfig = dic,
				AnalyticsConnectionString =
					Encryption.EncryptStringToBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1,
						EncryptionConstants.Image2)
			};
			return ret;
		}
	}
}