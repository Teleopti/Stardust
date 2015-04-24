using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IReadNHibFiles
	{
		IDictionary<string, DataSourceConfiguration> Read();
	}
}