using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config
{
	public interface IReadDataSourceConfiguration
	{
		IDictionary<string, DataSourceConfiguration> Read();
	}
}