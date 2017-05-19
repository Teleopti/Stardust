using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.TestCommon
{
	public class MessageBrokerCompositeDummy : IMessageBrokerComposite
	{
		private int _sentCount;

		public MessageBrokerCompositeDummy()
		{
			IsAlive = true;
			IsPollingAlive = true;
		}

		public void Dispose()
		{
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType,
			DomainUpdateType updateType, byte[] domainObject, Guid? trackId = null)
		{
			_sentCount++;
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_sentCount++;
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			_sentCount++;
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
		}

		public void Send(Message message)
		{
			_sentCount++;
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			foreach (var message in messages)
			{
				_sentCount++;
			}
		}

		public bool IsAlive { get; set; }
		public bool IsPollingAlive { get; set; }

		public string ServerUrl { get; set; }

		public void StartBrokerService(bool useLongPolling = false)
		{
		}

		public int SentCount()
		{
			return _sentCount;
		}
	}
}