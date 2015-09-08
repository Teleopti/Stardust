using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class InternalServiceBusSender : IServiceBusSender
	{
		private readonly Func<IServiceBus> _serviceBus;

		public InternalServiceBusSender(Func<IServiceBus> serviceBus)
		{
			_serviceBus = serviceBus;
		}

		public void Dispose()
		{
		}

		public void Send(bool throwOnNoBus, params object[] message)
		{
			_serviceBus().Send(message);
		}

		public bool EnsureBus()
		{
			return true;
		}
	}

	public class MessageSenderCreator
	{
		private readonly IServiceBusSender _serviceBusSender;
		private readonly IToggleManager _toggleManager;
		private readonly Interfaces.MessageBroker.Client.IMessageSender _messageSender;
		private readonly IJsonSerializer _serializer;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public MessageSenderCreator(IServiceBusSender serviceBusSender, 
			IToggleManager toggleManager, 
			Interfaces.MessageBroker.Client.IMessageSender messageSender,
			IJsonSerializer serializer, 
			ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_serviceBusSender = serviceBusSender;
			_toggleManager = toggleManager;
			_messageSender = messageSender;
			_serializer = serializer;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public ICurrentMessageSenders Create()
		{
			var populator = EventContextPopulator.Make();
			var businessUnit = CurrentBusinessUnit.Instance;
			var messageSender = new MessagePopulatingServiceBusSender(_serviceBusSender, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(_serviceBusSender), populator);
			var senders = new List<IMessageSender>
			{
				new ScheduleMessageSender(eventPublisher, new ClearEvents()),
				new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
				new MeetingMessageSender(eventPublisher),
				new GroupPageChangedMessageSender(messageSender),
				new TeamOrSiteChangedMessageSender(eventPublisher, businessUnit),
				new PersonChangedMessageSender(eventPublisher, businessUnit),
				new PersonPeriodChangedMessageSender(messageSender)
			};
			if (_toggleManager.IsEnabled(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				senders.Add(new AggregatedScheduleChangeMessageSender(_messageSender, CurrentDataSource.Make(), businessUnit, _serializer,
					_initiatorIdentifier));
			return new CurrentMessageSenders(senders);
		}
	}
}
