using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAggregateRootWithEvents : IAggregateRoot
	{
		void NotifyCommandId(Guid commandId);
		void NotifyTransactionComplete(DomainUpdateType operation);
		void NotifyDelete();
		IEnumerable<IEvent> PopAllEvents();
		bool HasEvents();
	}
}