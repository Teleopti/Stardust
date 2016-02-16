using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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