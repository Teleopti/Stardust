using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class DataSourceConfigurationProviderUsingNhibFiles : IDataSourceConfigurationProvider
	{
		private readonly IReadNHibFiles _readNHibFiles;
		private readonly INhibConfigurationEncryption _nhibConfigurationEncryption;
		private IDictionary<string, DataSourceConfiguration> allConfigs;

		public DataSourceConfigurationProviderUsingNhibFiles(IReadNHibFiles readNHibFiles, INhibConfigurationEncryption nhibConfigurationEncryption)
		{
			_readNHibFiles = readNHibFiles;
			_nhibConfigurationEncryption = nhibConfigurationEncryption;
		}

		public DataSourceConfiguration ForTenant(string tenant)
		{
			if(allConfigs==null)
				allConfigs = _readNHibFiles.Read();
	
			DataSourceConfiguration ret;
			if (allConfigs.TryGetValue(tenant, out ret))
			{
				return _nhibConfigurationEncryption.EncryptConfig(ret);
			}
				
			return null;
		}
	}
}