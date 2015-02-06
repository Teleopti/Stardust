using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class DataSourceConfigurationProviderUsingNhibFiles : IDataSourceConfigurationProvider
	{
		private readonly IReadNHibFiles _readNHibFiles;
		private IDictionary<string, DataSourceConfiguration> allConfigs;

		public DataSourceConfigurationProviderUsingNhibFiles(IReadNHibFiles readNHibFiles)
		{
			_readNHibFiles = readNHibFiles;
		}

		public DataSourceConfiguration ForTenant(string tenant)
		{
			if(allConfigs==null)
				allConfigs = _readNHibFiles.Read();
		
			//for now - return first one!
			//DataSourceConfiguration ret;
			//return allConfigs.TryGetValue(tenant, out ret) ? ret : null;
			return allConfigs.Select(x => x.Value).FirstOrDefault();
		}
	}
}