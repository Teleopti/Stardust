using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IReadDataSourceConfiguration
	{
		IDictionary<string, DataSourceConfiguration> Read();
	}
}