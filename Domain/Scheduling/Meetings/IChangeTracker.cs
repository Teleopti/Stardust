using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
	public interface IChangeTracker<T>
	{
		IEnumerable<IRootChangeInfo> CustomChanges(T afterChanges, DomainUpdateType status);
		void TakeSnapshot(T beforeChanges);
		void ResetSnapshot();
		T BeforeChanges();
	}
}