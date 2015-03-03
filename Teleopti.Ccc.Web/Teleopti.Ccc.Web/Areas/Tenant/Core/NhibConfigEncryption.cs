﻿using System.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface INhibConfigurationEncryption
	{
		DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig);
		DataSourceConfiguration DecryptConfig(DataSourceConfiguration dataSourceConfig);
	}
	public class NhibConfigurationEncryption : INhibConfigurationEncryption
	{
		public DataSourceConfiguration EncryptConfig(DataSourceConfiguration dataSourceConfig)
		{
			var dic = dataSourceConfig.ApplicationNHibernateConfig.Keys.ToDictionary(key => key, key => Encryption.EncryptStringToBase64(dataSourceConfig.ApplicationNHibernateConfig[key], EncryptionConstants.Image1, EncryptionConstants.Image2));
			dataSourceConfig.ApplicationNHibernateConfig = dic;
			dataSourceConfig.AnalyticsConnectionString = Encryption.EncryptStringToBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);
			return dataSourceConfig;
		}

		public DataSourceConfiguration DecryptConfig(DataSourceConfiguration dataSourceConfig)
		{
			dataSourceConfig.ApplicationNHibernateConfig.DecryptDictionary(EncryptionConstants.Image1, EncryptionConstants.Image2);
			dataSourceConfig.AnalyticsConnectionString = Encryption.DecryptStringFromBase64(dataSourceConfig.AnalyticsConnectionString, EncryptionConstants.Image1, EncryptionConstants.Image2);
			return dataSourceConfig;
		}
	}
}