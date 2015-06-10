using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationProviderUsingNhibFiles : IDataSourceConfigurationProvider
	{
		private readonly IReadDataSourceConfiguration _readDataSourceConfiguration;
		private readonly INhibConfigurationEncryption _nhibConfigurationEncryption;
		private IDictionary<string, DataSourceConfiguration> allConfigs;

		public DataSourceConfigurationProviderUsingNhibFiles(IReadDataSourceConfiguration readDataSourceConfiguration, INhibConfigurationEncryption nhibConfigurationEncryption)
		{
			_readDataSourceConfiguration = readDataSourceConfiguration;
			_nhibConfigurationEncryption = nhibConfigurationEncryption;
		}

		//TODO: tenant better to do encryption once!
		public DataSourceConfiguration ForTenant(Tenant tenant)
		{
			if(allConfigs==null)
				allConfigs = _readDataSourceConfiguration.Read();
	
			DataSourceConfiguration ret;
			return allConfigs.TryGetValue(tenant.Name, out ret) ? 
				_nhibConfigurationEncryption.EncryptConfig(ret) : null;
		}
	}
}