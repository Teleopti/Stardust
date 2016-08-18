using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IQueuedAbsenceRequest  :IAggregateRoot
	{
		IPersonRequest PersonRequest { get; set; }
		DateTime Created { get; set; }

		DateTime StartDateTime { get; set; }

		DateTime EndDateTime { get; set; }
	}
}