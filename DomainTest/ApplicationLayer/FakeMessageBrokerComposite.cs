using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
    public class FakeMessageBrokerComposite : IMessageBrokerComposite
    {
        private int _sentCount;
        private Type _messageType;
        private Guid _referenceObjectId;
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
            Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType,
            DomainUpdateType updateType, byte[] domainObject, Guid? trackId = null)
        {
            _sentCount++;
            _messageType = domainObjectType;
            _referenceObjectId = referenceObjectId;
        }

        public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
            Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
        {
            _sentCount++;
            _messageType = domainObjectType;
        }

        public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
        {
            throw new NotImplementedException();
        }

        public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
        {
            throw new NotImplementedException();
        }

        public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
        {
            throw new NotImplementedException();
        }

        public void Send(Message message)
        {
            throw new NotImplementedException();
        }

        public void SendMultiple(IEnumerable<Message> messages)
        {
            throw new NotImplementedException();
        }

        public bool IsAlive { get; set; }
        public bool IsPollingAlive { get; set; }
        public string ServerUrl { get; set; }
        public void StartBrokerService(bool useLongPolling = false)
        {
            throw new NotImplementedException();
        }

        public int SentCount()
        {
            return _sentCount;
        }
        public Type SentMessageType()
        {
            return _messageType;
        }

        public Guid ReferenceObjectId()
        {
            return _referenceObjectId;
            ;
        }
    }
}
