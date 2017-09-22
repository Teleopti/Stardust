using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
	public interface IScheduleScreenRefresher 
	{
		void Refresh(IScheduleDictionary scheduleDictionary, IEnumerable<IEventMessage> messageQueue, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflict> conflictsBuffer, Func<Guid, bool> isRelevantPerson, bool loadRequests);
	}
}