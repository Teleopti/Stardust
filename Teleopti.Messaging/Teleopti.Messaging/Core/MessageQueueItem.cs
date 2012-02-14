using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Core
{

    public class MessageQueueItem
    {
        private Guid _subscriberId;
        private IEventMessage _eventMessage;

        public MessageQueueItem(Guid subscriberId, IEventMessage eventMessage)
        {
            _subscriberId = subscriberId;
            _eventMessage = eventMessage;
        }

        public Guid SubscriberId
        {
            get { return _subscriberId; }
        }

        public IEventMessage EventMessage
        {
            get { return _eventMessage; }
        }
    }
}