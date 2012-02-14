﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleScreenRefresher : IScheduleScreenRefresher
    {
        private readonly IOwnMessageQueue _messageQueueUpdater;
        private readonly IScheduleDataRefresher _scheduleDataRefresher;
        private readonly IMeetingRefresher _meetingRefresher;
        private readonly IPersonRequestRefresher _personRequestRefresher;

        public ScheduleScreenRefresher(IOwnMessageQueue messageQueueUpdater, IScheduleDataRefresher scheduleDataRefresher, IMeetingRefresher meetingRefresher, IPersonRequestRefresher personRequestRefresher) 
		{
            _messageQueueUpdater = messageQueueUpdater;
            _scheduleDataRefresher = scheduleDataRefresher;
            _meetingRefresher = meetingRefresher;
            _personRequestRefresher = personRequestRefresher;
        }

        public void Refresh(IScheduleDictionary scheduleDictionary, IList<IEventMessage> messageQueue, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflictMessageState> conflictsBuffer)
        {
            _messageQueueUpdater.ReassociateDataWithAllPeople();

            var scheduleDataMessages = QueryMessagesByType<IPersistableScheduleData>(messageQueue);
            _scheduleDataRefresher.Refresh(scheduleDictionary, messageQueue, scheduleDataMessages, refreshedEntitiesBuffer, conflictsBuffer);

            var meetingMessages = QueryMessagesByType<IMeeting>(messageQueue);
            _meetingRefresher.Refresh(messageQueue, meetingMessages);

            var personRequestMessages = QueryMessagesByType<IPersonRequest>(messageQueue);
            _personRequestRefresher.Refresh(messageQueue, personRequestMessages);

            _messageQueueUpdater.NotifyMessageQueueSize();
        }

        private static IEnumerable<IEventMessage> QueryMessagesByType<T>(IEnumerable<IEventMessage> messageQueue)
        {
            return (from m in messageQueue where typeof(T).IsAssignableFrom(m.InterfaceType) select m).ToArray();
        }
    }
}
