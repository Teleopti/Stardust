using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Infrastructure.ServiceBus
{
	public class MessageSenderCreator
	{
		private readonly IToggleManager _toggleManager;
		private readonly IMessagePopulatingServiceBusSender _messagePopulatingServiceBusSender;
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;
		private readonly IMessageSender _messageSender;
		private readonly IJsonSerializer _serializer;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		public MessageSenderCreator(
			IToggleManager toggleManager,
			IMessageSender messageSender,
			IMessagePopulatingServiceBusSender messagePopulatingServiceBusSender,
			IEventPopulatingPublisher eventPopulatingPublisher,
			IJsonSerializer serializer,
			ICurrentInitiatorIdentifier initiatorIdentifier)
		{
			_toggleManager = toggleManager;
			_messageSender = messageSender;
			_messagePopulatingServiceBusSender = messagePopulatingServiceBusSender;
			_eventPopulatingPublisher = eventPopulatingPublisher;
			_serializer = serializer;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public CurrentPersistCallbacks Create()
		{
			var businessUnit = CurrentBusinessUnit.Make();
			var currentDataSource = CurrentDataSource.Make();
			var senders = new List<IPersistCallback>
			{
				new ScheduleChangedEventPublisher(_eventPopulatingPublisher, new ClearEvents()),
				new EventsMessageSender(new SyncEventsPublisher(_eventPopulatingPublisher)),
				new ScheduleChangedEventFromMeetingPublisher(_eventPopulatingPublisher),
				new GroupPageChangedBusMessageSender(_messagePopulatingServiceBusSender),
				new PersonCollectionChangedEventPublisherForTeamOrSite(_eventPopulatingPublisher, businessUnit),
				new PersonCollectionChangedEventPublisher(_eventPopulatingPublisher, businessUnit),
				new PersonPeriodChangedBusMessagePublisher(_messagePopulatingServiceBusSender)
			};
			
			if (_toggleManager.IsEnabled(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
			{
				senders.Add(new ScheduleChangedMessageSender(_messageSender, currentDataSource,
					businessUnit, _serializer, _initiatorIdentifier));
			}

			return new CurrentPersistCallbacks(senders);
		}
	}
}
