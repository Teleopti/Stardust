using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Persisters.NewStuff;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
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