using System;
using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
    public interface IUpdatePersonRequestsFromMessages
    {
        void UpdatePersonRequest(IEventMessage eventMessage);
    }

    public interface IPersonRequestRefresher {
        void Refresh(IList<IEventMessage> messageQueue, IEnumerable<IEventMessage> meetingMessages);
    }

    public class PersonRequestRefresher : IPersonRequestRefresher
    {
        private readonly IUpdatePersonRequestsFromMessages _personRequestFromMessagesUpdater;

        public PersonRequestRefresher(IUpdatePersonRequestsFromMessages personRequestFromMessagesUpdater)
        {
            _personRequestFromMessagesUpdater = personRequestFromMessagesUpdater;
        }

        public void Refresh(IList<IEventMessage> messageQueue, IEnumerable<IEventMessage> requestMessages)
        {
	        var alreadyRefreshedRequestIds = new HashSet<Guid>();
	        foreach (var requestMessage in requestMessages)
	        {
		        if (alreadyRefreshedRequestIds.Add(requestMessage.DomainObjectId))
		        {
			        _personRequestFromMessagesUpdater.UpdatePersonRequest(requestMessage);
		        }
		        messageQueue.Remove(requestMessage);
	        }
        }
    }
}