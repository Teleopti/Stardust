﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
    public interface IScheduleRefresher
    {
	    void Refresh(IScheduleDictionary scheduleDictionary,
	                 IEnumerable<IEventMessage> scheduleMessages,
					 ICollection<IPersistableScheduleData> refreshedEntitiesBuffer,
	                 ICollection<PersistConflict> conflictsBuffer, Func<Guid,bool> isRelevantPerson);
    }
}