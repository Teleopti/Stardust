using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
	public interface IUpdateScheduleDataFromMessages
    {
		IPersistableScheduleData DeleteScheduleData(IEventMessage eventMessage);
		IPersistableScheduleData UpdateInsertScheduleData(IEventMessage eventMessage);
		void FillReloadedScheduleData(IPersistableScheduleData databaseVersionOfEntity);
		void NotifyMessageQueueSizeChange();
	}
}