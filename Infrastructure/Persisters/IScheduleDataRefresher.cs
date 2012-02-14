using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleDataRefresher 
	{
		void Refresh(IScheduleDictionary scheduleDictionary, IList<IEventMessage> messageQueue, IEnumerable<IEventMessage> scheduleDataMessages, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflictMessageState> conflictsBuffer);
	}
}