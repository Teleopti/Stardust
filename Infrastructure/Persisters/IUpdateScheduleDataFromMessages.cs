﻿using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IUpdateScheduleDataFromMessages
    {
		IPersistableScheduleData DeleteScheduleData(IEventMessage eventMessage);
		IPersistableScheduleData UpdateInsertScheduleData(IEventMessage eventMessage);
		void FillReloadedScheduleData(IPersistableScheduleData databaseVersionOfEntity);
		void NotifyMessageQueueSize();
	}
}