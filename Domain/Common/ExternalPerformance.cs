using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Common
{
	public class ExternalPerformance: AggregateRoot
	{
		public int ExternalId { get; set; }
		public string Name { get; set; }
		public ExternalPerformanceDataType DataType { get; set; }
	}
}
