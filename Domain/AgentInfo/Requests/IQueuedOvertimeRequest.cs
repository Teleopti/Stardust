using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IQueuedOvertimeRequest : IAggregateRoot
	{
		Guid PersonRequest { get; set; }
		DateTime Created { get; set; }
		DateTime StartDateTime { get; set; }
		DateTime EndDateTime { get; set; }
		DateTime? Sent { get; set; }
	}
}
