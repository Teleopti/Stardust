using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
	public class ScheduleDataRefresher : IScheduleDataRefresher
    {
        private readonly IScheduleStorage _scheduleStorage;
	    private readonly IUpdateScheduleDataFromMessages _scheduleDataUpdater;
		private readonly IMessageQueueRemoval _messageQueueRemoval;

		public ScheduleDataRefresher(IScheduleStorage scheduleStorage, IUpdateScheduleDataFromMessages scheduleDataUpdater, IMessageQueueRemoval messageQueueRemoval)
        {
            _scheduleStorage = scheduleStorage;
            _scheduleDataUpdater = scheduleDataUpdater;
	        _messageQueueRemoval = messageQueueRemoval;
        }

	    public void Refresh(IScheduleDictionary scheduleDictionary, 
	                        IEnumerable<IEventMessage> scheduleDataMessages,
							ICollection<IPersistableScheduleData> refreshedEntitiesBuffer,
	                        ICollection<PersistConflict> conflictsBuffer)
	    {
	        var myChanges = scheduleDictionary.DifferenceSinceSnapshot();

            foreach (var eventMessage in scheduleDataMessages)
            {
                var myVersionOfEntity = myChanges.FindItemByOriginalId(eventMessage.DomainObjectId);

                if (myVersionOfEntity.HasValue)
                {
                    var databaseVerionOfEntity =
                        _scheduleStorage.LoadScheduleDataAggregate(eventMessage.InterfaceType,
                                                                      eventMessage.DomainObjectId);
                    _scheduleDataUpdater.FillReloadedScheduleData(databaseVerionOfEntity);
	                var state = new PersistConflict(myVersionOfEntity.Value, databaseVerionOfEntity);
                    conflictsBuffer.Add(state);
                    continue;
                }

                var messageVersionOfEntity = eventMessage.DomainUpdateType ==
                                                                  DomainUpdateType.Delete
                                                                      ? _scheduleDataUpdater.DeleteScheduleData(
                                                                          eventMessage)
                                                                      : _scheduleDataUpdater.UpdateInsertScheduleData(
                                                                          eventMessage);
                if (messageVersionOfEntity != null)
                    refreshedEntitiesBuffer.Add(messageVersionOfEntity);
              
							_messageQueueRemoval.Remove(eventMessage);

            }
	    }
    }
}