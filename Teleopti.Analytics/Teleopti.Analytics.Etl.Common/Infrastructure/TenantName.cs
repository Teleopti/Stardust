using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class TenantName:ITenantName
	{
		public string DataSourceName { get; set; }
	}
}
