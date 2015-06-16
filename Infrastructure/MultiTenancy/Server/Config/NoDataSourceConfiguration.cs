using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config
{
	public class NoDataSourceConfiguration : IReadDataSourceConfiguration
	{
		public IDictionary<string, DataSourceConfiguration> Read()
		{
			return new Dictionary<string, DataSourceConfiguration>();
		}
	}
}