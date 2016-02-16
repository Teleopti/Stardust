using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class MessageBrokerCompositeDummy : IMessageBrokerComposite
	{
		public void Dispose()
		{
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType,
			DomainUpdateType updateType, byte[] domainObject, Guid? trackId = null)
		{
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
		}

		public void Send(Message message)
		{
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
		}

		public bool IsAlive { get; }
		public bool IsPollingAlive { get; }
		public string ServerUrl { get; set; }

		public void StartBrokerService(bool useLongPolling = false)
		{
		}
	}
}