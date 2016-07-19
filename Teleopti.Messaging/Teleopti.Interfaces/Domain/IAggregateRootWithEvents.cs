using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAggregateRootWithEvents : IAggregateRoot
	{
		void NotifyCommandId(Guid commandId);
		void NotifyTransactionComplete(DomainUpdateType operation);
		void NotifyDelete();
		IEnumerable<IEvent> PopAllEvents();
	}
}