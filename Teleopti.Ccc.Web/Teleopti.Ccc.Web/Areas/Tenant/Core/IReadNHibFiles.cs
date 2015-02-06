using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IReadNHibFiles
	{
		IDictionary<string, DataSourceConfiguration> Read();
	}
}