using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
    public interface IUpdateMeetingsFromMessages
    {
        void UpdateMeeting(IEventMessage eventMessage);
    }

    public interface IMeetingRefresher {
        void Refresh(IList<IEventMessage> messageQueue, IEnumerable<IEventMessage> meetingMessages);
    }

    public class MeetingRefresher : IMeetingRefresher
    {
        private readonly IUpdateMeetingsFromMessages _meetingUpdater;

        public MeetingRefresher(IUpdateMeetingsFromMessages meetingUpdater)
        {
            _meetingUpdater = meetingUpdater;
        }

        public void Refresh(IList<IEventMessage> messageQueue, IEnumerable<IEventMessage> meetingMessages)
        {
            foreach (var meetingMessage in meetingMessages)
            {
                _meetingUpdater.UpdateMeeting(meetingMessage);
                messageQueue.Remove(meetingMessage);
            }
        }

    }
}