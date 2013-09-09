using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleDataRefresher : IScheduleDataRefresher
    {
        private readonly IScheduleRepository _scheduleRepository;
	    private readonly IUpdateScheduleDataFromMessages _scheduleDataUpdater;

        public ScheduleDataRefresher(IScheduleRepository scheduleRepository, IUpdateScheduleDataFromMessages scheduleDataUpdater)
        {
            _scheduleRepository = scheduleRepository;
            _scheduleDataUpdater = scheduleDataUpdater;
        }

	    public void Refresh(IScheduleDictionary scheduleDictionary, IList<IEventMessage> messageQueue,
	                        IEnumerable<IEventMessage> scheduleDataMessages,
	                        ICollection<IPersistableScheduleData> refreshedEntitiesBuffer,
	                        ICollection<PersistConflictMessageState> conflictsBuffer)
	    {
	        var myChanges = scheduleDictionary.DifferenceSinceSnapshot();

            foreach (var eventMessage in scheduleDataMessages)
            {
                var myVersionOfEntity = myChanges.FindItemByOriginalId(eventMessage.DomainObjectId);

                if (myVersionOfEntity.HasValue)
                {
                    var databaseVerionOfEntity =
                        _scheduleRepository.LoadScheduleDataAggregate(eventMessage.InterfaceType,
                                                                      eventMessage.DomainObjectId);
                    _scheduleDataUpdater.FillReloadedScheduleData(databaseVerionOfEntity);
                    var state = new PersistConflictMessageState(myVersionOfEntity.Value, databaseVerionOfEntity,
                                                                eventMessage, m => RemoveFromQueue(messageQueue, m));
                    conflictsBuffer.Add(state);
                    continue;
                }

                IPersistableScheduleData messageVersionOfEntity = eventMessage.DomainUpdateType ==
                                                                  DomainUpdateType.Delete
                                                                      ? _scheduleDataUpdater.DeleteScheduleData(
                                                                          eventMessage)
                                                                      : _scheduleDataUpdater.UpdateInsertScheduleData(
                                                                          eventMessage);
                if (messageVersionOfEntity != null)
                    refreshedEntitiesBuffer.Add(messageVersionOfEntity);
                //denna gör lite fel idag, ska ta hänsyn till både gammalt och nytt
                messageQueue.Remove(eventMessage);
            }
	    }

	    private void RemoveFromQueue(ICollection<IEventMessage> messageQueue, IEventMessage m) 
		{
            messageQueue.Remove(m);
            _scheduleDataUpdater.NotifyMessageQueueSizeChange();
        }
    }
}