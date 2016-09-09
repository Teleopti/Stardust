using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IQueuedAbsenceRequest  :IAggregateRoot
	{
		Guid PersonRequest { get; set; }
		DateTime Created { get; set; }

		DateTime StartDateTime { get; set; }

		DateTime EndDateTime { get; set; }

		DateTime? Sent { get; set; }
	}
}