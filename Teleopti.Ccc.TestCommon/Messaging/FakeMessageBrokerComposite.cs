using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.TestCommon.Messaging
{
	public class FakeMessageBrokerComposite : IMessageBrokerComposite
	{
		private int _sendInvokedCount;
		private bool _disabled;
		private List<Message> _messages = new List<Message>();
		public void Dispose()
		{
		}

		public void ResetSendInvokedCount()
		{
			_sendInvokedCount = 0;
		}

		public void Disable()
		{
			_disabled = true;
		}

		public void Enable()
		{
			_disabled = false;
		}

		public int GetSendInvokedCount()
		{
			return _sendInvokedCount;
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType,
			DomainUpdateType updateType, byte[] domainObject, Guid? trackId = null)
		{
			if (!_disabled) {
				_sendInvokedCount++;
				_messages.Add(new Message {
					DataSource = dataSource,
					BusinessUnitId = businessUnitId.ToString(),
					StartDate = Subscription.DateToString(eventStartDate),
					EndDate = Subscription.DateToString(eventEndDate),
					DomainType = domainObjectType.Name
				});
			}
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			if (!_disabled)
			{
				_sendInvokedCount++;
			}
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			if (!_disabled) _sendInvokedCount++;
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

		public bool IsAlive { get; private set; }
		public bool IsPollingAlive { get; private set; }
		public string ServerUrl { get; set; }
		public void StartBrokerService(bool useLongPolling = false)
		{
			throw new NotImplementedException();
		}

		public List<Message> GetMessages()
		{
			return _messages;
		}

	}
}
