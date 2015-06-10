﻿using System.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationEncryption : IDataSourceConfigurationEncryption
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