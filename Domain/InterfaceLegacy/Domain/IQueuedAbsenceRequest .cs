using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IQueuedAbsenceRequest : IAggregateRoot
	{
		Guid PersonRequest { get; set; }
		DateTime Created { get; set; }

		DateTime StartDateTime { get; set; }

		DateTime EndDateTime { get; set; }

		DateTime? Sent { get; set; }

		RequestValidatorsFlag MandatoryValidators { get; set; }
	}
}