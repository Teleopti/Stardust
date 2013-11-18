using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
	public interface IUpdateScheduleDataFromMessages
    {
		INonversionedPersistableScheduleData DeleteScheduleData(IEventMessage eventMessage);
		INonversionedPersistableScheduleData UpdateInsertScheduleData(IEventMessage eventMessage);
		void FillReloadedScheduleData(INonversionedPersistableScheduleData databaseVersionOfEntity);
		void NotifyMessageQueueSizeChange();
	}
}