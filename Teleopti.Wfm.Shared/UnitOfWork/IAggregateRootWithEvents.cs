using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAggregateRootWithEvents : IAggregateRoot
	{
		void NotifyCommandId(Guid commandId);
		void NotifyTransactionComplete(DomainUpdateType operation);
		void NotifyDelete();
		IEnumerable<IEvent> PopAllEvents(IPopEventsContext context);
		bool HasEvents();
	}
}