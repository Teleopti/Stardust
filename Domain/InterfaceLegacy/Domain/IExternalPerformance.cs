using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IExternalPerformance : IAggregateRoot, IDeleteTag
	{
		int ExternalId { get; set; }
		string Name { get; set; }
		ExternalPerformanceDataType DataType { get; set; }
	}
}