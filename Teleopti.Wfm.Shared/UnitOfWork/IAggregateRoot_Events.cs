using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public interface IAggregateRoot_Events : IAggregateRoot
	{
		void NotifyCommandId(Guid commandId);
		void NotifyTransactionComplete(DomainUpdateType operation);
		void NotifyDelete();
		IEnumerable<IEvent> PopAllEvents(IPopEventsContext context);
		bool HasEvents();
	}
}