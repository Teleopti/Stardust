using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Messaging.Client.Composite
{
	public class MessageBrokerCompositeClient : IMessageBrokerComposite
	{
		private readonly ISignalRClient _signalRClient;
		private readonly IMessageSender _messageSender;
		private readonly IMessageListener _signalRMessageListener;
		private readonly MessageCreator _messageCreator;
		private readonly HttpListener _mailboxListener;

		public MessageBrokerCompositeClient(
			IMessageFilterManager typeFilter, 
			ISignalRClient signalRClient, 
			IMessageSender messageSender, 
			IJsonDeserializer deserializer, 
			ITime time, 
			IConfigReader config, 
			HttpClientM client, 
            IMessageBrokerUrl url)
		{
			_signalRClient = signalRClient;
			_messageSender = messageSender;
			_signalRMessageListener = new SignalRListener(_signalRClient);
			_mailboxListener = new HttpListener(deserializer, time, config, client, url);
			_messageCreator = new MessageCreator(messageSender, typeFilter);
		}

		public void StartBrokerService(bool useLongPolling = false)
		{
			_signalRClient.StartBrokerService(useLongPolling);
		}

		public bool IsAlive
		{
			get { return _signalRClient.IsAlive; }
		}

		public bool IsPollingAlive
		{
			get { return _mailboxListener.IsAlive(); }
		}

		public void Dispose()
		{
			_signalRClient.Dispose();
			_mailboxListener.Dispose();
		}

		public string ServerUrl { get { return _signalRClient.Url; } set { _signalRClient.Configure(value); } }

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject, Guid? trackId = null)
		{
			_messageCreator.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, domainObject, trackId);
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_messageCreator.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			_messageCreator.Send(dataSource, businessUnitId, eventMessages);
		}

		public void Send(Message message)
		{
			_messageSender.Send(message);
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			_messageSender.SendMultiple(messages);
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			if (subscription.MailboxId == null)
				_signalRMessageListener.RegisterSubscription(subscription, eventMessageHandler);
			else
				_mailboxListener.RegisterSubscription(subscription, eventMessageHandler);
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_signalRMessageListener.UnregisterSubscription(eventMessageHandler);
			_mailboxListener.UnregisterSubscription(eventMessageHandler);
		}

	}

}
