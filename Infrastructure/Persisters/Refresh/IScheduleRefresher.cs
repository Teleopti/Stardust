﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

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