using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationProvider : IDataSourceConfigurationProvider
	{
		private readonly IReadDataSourceConfiguration _readDataSourceConfiguration;
		private readonly IDataSourceConfigurationEncryption _dataSourceConfigurationEncryption;
		private IDictionary<string, DataSourceConfiguration> allConfigs;

		public DataSourceConfigurationProvider(IReadDataSourceConfiguration readDataSourceConfiguration, IDataSourceConfigurationEncryption dataSourceConfigurationEncryption)
		{
			_readDataSourceConfiguration = readDataSourceConfiguration;
			_dataSourceConfigurationEncryption = dataSourceConfigurationEncryption;
		}

		//TODO: tenant better to do encryption once!
		public DataSourceConfiguration ForTenant(Tenant tenant)
		{
			if(allConfigs==null)
				allConfigs = _readDataSourceConfiguration.Read();
	
			DataSourceConfiguration ret;
			return allConfigs.TryGetValue(tenant.Name, out ret) ? 
				_dataSourceConfigurationEncryption.EncryptConfig(ret) : null;
		}
	}
}