using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
	public class ScheduleScreenRefresher : IScheduleScreenRefresher
    {
        private readonly IReassociateDataForSchedules _messageQueueUpdater;
	    private readonly IScheduleRefresher _scheduleRefresher;
	    private readonly IScheduleDataRefresher _scheduleDataRefresher;
        private readonly IMeetingRefresher _meetingRefresher;
        private readonly IPersonRequestRefresher _personRequestRefresher;

		public ScheduleScreenRefresher(IReassociateDataForSchedules messageQueueUpdater, IScheduleRefresher scheduleRefresher, IScheduleDataRefresher scheduleDataRefresher, IMeetingRefresher meetingRefresher, IPersonRequestRefresher personRequestRefresher) 
		{
            _messageQueueUpdater = messageQueueUpdater;
            _scheduleRefresher = scheduleRefresher;
            _scheduleDataRefresher = scheduleDataRefresher;
            _meetingRefresher = meetingRefresher;
            _personRequestRefresher = personRequestRefresher;
		}

		public void Refresh(IScheduleDictionary scheduleDictionary, IEnumerable<IEventMessage> messageQueue,
			ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflict> conflictsBuffer,
			Func<Guid, bool> isRelevantPerson, bool loadRequests)
		{
			_messageQueueUpdater.ReassociateDataForAllPeople();

			var scheduleMessages = QueryMessagesByType<IScheduleChangedEvent>(messageQueue);
			_scheduleRefresher.Refresh(scheduleDictionary, scheduleMessages, refreshedEntitiesBuffer, conflictsBuffer,
				isRelevantPerson);

			var scheduleDataMessages = QueryMessagesByType<IPersistableScheduleData>(messageQueue);
			_scheduleDataRefresher.Refresh(scheduleDictionary, scheduleDataMessages, refreshedEntitiesBuffer, conflictsBuffer);

			var meetingMessages = QueryMessagesByType<IMeeting>(messageQueue);
			_meetingRefresher.Refresh(meetingMessages);

			var personRequestMessages = QueryMessagesByType<IPersonRequest>(messageQueue);
			if (personRequestMessages.Any())
			{
				if (loadRequests)
					_personRequestRefresher.Refresh(personRequestMessages);
				else
					_personRequestRefresher.RemoveWithoutRefresh(personRequestMessages);
			}
			

		}

		private static IEnumerable<IEventMessage> QueryMessagesByType<T>(IEnumerable<IEventMessage> messageQueue)
        {
            return messageQueue.Where(m => typeof(T).IsAssignableFrom(m.InterfaceType)).ToArray();
        }
    }
}
