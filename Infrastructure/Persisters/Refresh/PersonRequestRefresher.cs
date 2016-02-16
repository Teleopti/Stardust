using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
    public interface IUpdatePersonRequestsFromMessages
    {
        void UpdatePersonRequest(IEventMessage eventMessage);
    }

    public interface IPersonRequestRefresher {
        void Refresh(IEnumerable<IEventMessage> meetingMessages);
    }

    public class PersonRequestRefresher : IPersonRequestRefresher
    {
        private readonly IUpdatePersonRequestsFromMessages _personRequestFromMessagesUpdater;
	    private readonly IMessageQueueRemoval _messageQueueRemoval;

	    public PersonRequestRefresher(IUpdatePersonRequestsFromMessages personRequestFromMessagesUpdater, IMessageQueueRemoval messageQueueRemoval)
        {
	        _personRequestFromMessagesUpdater = personRequestFromMessagesUpdater;
	        _messageQueueRemoval = messageQueueRemoval;
        }

	    public void Refresh(IEnumerable<IEventMessage> requestMessages)
        {
	        var alreadyRefreshedRequestIds = new HashSet<Guid>();
	        foreach (var requestMessage in requestMessages)
	        {
		        if (alreadyRefreshedRequestIds.Add(requestMessage.DomainObjectId))
		        {
			        _personRequestFromMessagesUpdater.UpdatePersonRequest(requestMessage);
		        }
						_messageQueueRemoval.Remove(requestMessage);
	        }
        }
    }
}