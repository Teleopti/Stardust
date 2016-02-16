using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
    public interface IUpdateMeetingsFromMessages
    {
        void UpdateMeeting(IEventMessage eventMessage);
    }

    public interface IMeetingRefresher 
		{
        void Refresh(IEnumerable<IEventMessage> meetingMessages);
    }

    public class MeetingRefresher : IMeetingRefresher
    {
        private readonly IUpdateMeetingsFromMessages _meetingUpdater;
	    private readonly IMessageQueueRemoval _messageQueueRemoval;

	    public MeetingRefresher(IUpdateMeetingsFromMessages meetingUpdater, IMessageQueueRemoval messageQueueRemoval)
        {
	        _meetingUpdater = meetingUpdater;
	        _messageQueueRemoval = messageQueueRemoval;
        }

	    public void Refresh(IEnumerable<IEventMessage> meetingMessages)
        {
            foreach (var meetingMessage in meetingMessages)
            {
                _meetingUpdater.UpdateMeeting(meetingMessage);
							_messageQueueRemoval.Remove(meetingMessage);
            }
        }

    }
}