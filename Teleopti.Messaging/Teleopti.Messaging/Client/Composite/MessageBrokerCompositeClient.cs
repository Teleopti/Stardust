using System;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Messaging.Client.Composite
{
	public class MessageBrokerCompositeClient : IMessageBrokerComposite
	{
		private readonly ISignalRClient _signalRClient;
		private readonly IMessageListener _signalRMessageListener;
		private readonly MessageCreator _messageCreator;
		private readonly IMessageListener _mailboxListener;

		public MessageBrokerCompositeClient(
			IMessageFilterManager typeFilter, 
			ISignalRClient signalRClient,
			IMessageSender messageSender, 
			IJsonSerializer serializer, 
			IJsonDeserializer deserializer, 
			ITime time,
			IHttpServer httpServer, 
			IConfigurationWrapper configurationWrapper)
		{
			_signalRClient = signalRClient;
			var eventHandlers = new EventHandlers();
			_signalRMessageListener = new SignalRListener(_signalRClient, eventHandlers);
			_mailboxListener = new HttpListener(eventHandlers,
				httpServer, _signalRClient, serializer, deserializer, time, configurationWrapper);
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

		public void Dispose()
		{
			_signalRClient.Dispose();
		}

		public string ServerUrl { get { return _signalRClient.Url; } set { _signalRClient.Configure(value); } }

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_messageCreator.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_messageCreator.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			_messageCreator.Send(dataSource, businessUnitId, eventMessages);
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			if (subscription.MailboxId == null || subscription.MailboxId == Guid.Empty.ToString())
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
