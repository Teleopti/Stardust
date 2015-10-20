﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.ServiceBus
{
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

		public ICurrentPersistCallbacks Create()
		{
			var populator = EventContextPopulator.Make();
			var businessUnit = CurrentBusinessUnit.Make();
			var messageSender = new MessagePopulatingServiceBusSender(_serviceBusSender, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(_serviceBusSender), populator);
			var senders = new List<IPersistCallback>
			{
				new ScheduleChangedEventPublisher(eventPublisher, new ClearEvents()),
				new EventsMessageSender(new SyncEventsPublisher(eventPublisher)),
				new ScheduleChangedEventFromMeetingPublisher(eventPublisher),
				new GroupPageChangedBusMessageSender(messageSender),
				new PersonCollectionChangedEventPublisherForTeamOrSite(eventPublisher, businessUnit),
				new PersonCollectionChangedEventPublisher(eventPublisher, businessUnit),
				new PersonPeriodChangedBusMessagePublisher(messageSender)
			};
			if (_toggleManager.IsEnabled(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				senders.Add(new ScheduleChangedMessageSender(_messageSender, CurrentDataSource.Make(), businessUnit, _serializer,
					_initiatorIdentifier));
			return new CurrentPersistCallbacks(senders);
		}
	}
}
