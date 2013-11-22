using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleScreenRefresher : IScheduleScreenRefresher
    {
        private readonly IOwnMessageQueue _messageQueueUpdater;
	    private readonly IScheduleRefresher _scheduleRefresher;
	    private readonly IScheduleDataRefresher _scheduleDataRefresher;
        private readonly IMeetingRefresher _meetingRefresher;
        private readonly IPersonRequestRefresher _personRequestRefresher;

        public ScheduleScreenRefresher(IOwnMessageQueue messageQueueUpdater, IScheduleRefresher scheduleRefresher, IScheduleDataRefresher scheduleDataRefresher, IMeetingRefresher meetingRefresher, IPersonRequestRefresher personRequestRefresher) 
		{
            _messageQueueUpdater = messageQueueUpdater;
            _scheduleRefresher = scheduleRefresher;
            _scheduleDataRefresher = scheduleDataRefresher;
            _meetingRefresher = meetingRefresher;
            _personRequestRefresher = personRequestRefresher;
        }

        public void Refresh(IScheduleDictionary scheduleDictionary, IList<IEventMessage> messageQueue, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflictMessageState> conflictsBuffer, Func<Guid,bool> isRelevantPerson)
        {
            _messageQueueUpdater.ReassociateDataWithAllPeople();

            var scheduleMessages = QueryMessagesByType<IScheduleChangedEvent>(messageQueue);
            _scheduleRefresher.Refresh(scheduleDictionary, messageQueue, scheduleMessages, refreshedEntitiesBuffer,
                                       conflictsBuffer, isRelevantPerson);

            var scheduleDataMessages = QueryMessagesByType<IPersistableScheduleData>(messageQueue);
            _scheduleDataRefresher.Refresh(scheduleDictionary, messageQueue, scheduleDataMessages, refreshedEntitiesBuffer, conflictsBuffer);

            var meetingMessages = QueryMessagesByType<IMeeting>(messageQueue);
            _meetingRefresher.Refresh(messageQueue, meetingMessages);

            var personRequestMessages = QueryMessagesByType<IPersonRequest>(messageQueue);
            _personRequestRefresher.Refresh(messageQueue, personRequestMessages);

            _messageQueueUpdater.NotifyMessageQueueSizeChange();
        }

        private static IEnumerable<IEventMessage> QueryMessagesByType<T>(IEnumerable<IEventMessage> messageQueue)
        {
            return messageQueue.Where(m => typeof(T).IsAssignableFrom(m.InterfaceType)).ToArray();
        }
    }
}
