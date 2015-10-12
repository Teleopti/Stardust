using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
	public interface IScheduleScreenRefresher 
	{
		void Refresh(IScheduleDictionary scheduleDictionary, IEnumerable<IEventMessage> messageQueue, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflict> conflictsBuffer, Func<Guid,bool> isRelevantPerson);
	}
}