using System;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Composite
{
	public class SignalBroker : IMessageBroker
	{
		private readonly ISignalRClient _signalRClient;
		private readonly MessageBrokerListener _messageBrokerListener;
		private readonly MessageBrokerSender _messageBrokerSender;

		public SignalBroker(IMessageFilterManager typeFilter, ISignalRClient signalRClient, IMessageSender messageSender)
		{
			_signalRClient = signalRClient;
			_messageBrokerListener = new MessageBrokerListener(_signalRClient);
			_messageBrokerSender = new MessageBrokerSender(messageSender, typeFilter);
			_signalRClient.RegisterCallbacks(_messageBrokerListener.OnNotification, _messageBrokerListener.ReregisterSubscriptions);
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

		public string ServerUrl { get { return _signalRClient.ServerUrl; } set { _signalRClient.ServerUrl = value; } }

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_messageBrokerSender.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, referenceObjectId, referenceObjectType, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_messageBrokerSender.Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			_messageBrokerSender.Send(dataSource, businessUnitId, eventMessages);
		}

		public void Send(Notification notification)
		{
			_messageBrokerSender.Send(notification);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, domainObjectType);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, domainObjectType);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, domainObjectType, startDate, endDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, domainObjectId, domainObjectType, startDate, endDate);
		}

		public void RegisterEventSubscription(string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			_messageBrokerListener.RegisterEventSubscription(dataSource, businessUnitId, eventMessageHandler, referenceObjectId, referenceObjectType, domainObjectType, startDate, endDate);
		}

		public void UnregisterEventSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_messageBrokerListener.UnregisterEventSubscription(eventMessageHandler);
		}

	}

}
