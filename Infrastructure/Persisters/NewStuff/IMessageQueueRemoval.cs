﻿using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.NewStuff
{
	public interface IMessageQueueRemoval
	{
		void Remove(IEventMessage eventMessage);
		void Remove(PersistConflict persistConflict);
	}
}