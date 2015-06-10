using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IReadDataSourceConfiguration
	{
		IDictionary<string, DataSourceConfiguration> Read();
	}
}