using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleScreenRefresher 
	{
		void Refresh(IScheduleDictionary scheduleDictionary, IList<IEventMessage> messageQueue, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflictMessageState> conflictsBuffer, Func<Guid,bool> isRelevantPerson);
	}
}