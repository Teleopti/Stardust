using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentPersistCallbacks : ICurrentPersistCallbacks, IMessageSendersScope
	{
		private readonly IServiceBusSender _serviceBusSender;
		private readonly IToggleManager _toggleManager;
		private readonly Interfaces.MessageBroker.Client.IMessageSender _messageSender;
		private readonly IJsonSerializer _serializer;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;

		private static IEnumerable<IPersistCallback> _globalMessageSenders;
		[ThreadStatic]
		private static IEnumerable<IPersistCallback> _threadMessageSenders;
		private readonly List<IPersistCallback> _messageSenders;

		public CurrentPersistCallbacks(IServiceBusSender serviceBusSender,
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

			_messageSenders = new List<IPersistCallback>();
			initialize();
		}

		private void initialize()
		{
			var populator = EventContextPopulator.Make();
			var businessUnit = CurrentBusinessUnit.Make();
			var sender = new MessagePopulatingServiceBusSender(_serviceBusSender, populator);
			var eventPublisher = new EventPopulatingPublisher(new ServiceBusEventPublisher(_serviceBusSender), populator);
			_messageSenders.Add(new ScheduleChangedEventPublisher(eventPublisher, new ClearEvents()));
			_messageSenders.Add(new EventsMessageSender(new SyncEventsPublisher(eventPublisher)));
			_messageSenders.Add(new ScheduleChangedEventFromMeetingPublisher(eventPublisher));
			_messageSenders.Add(new GroupPageChangedBusMessageSender(sender));
			_messageSenders.Add(new PersonCollectionChangedEventPublisherForTeamOrSite(eventPublisher, businessUnit));
			_messageSenders.Add(new PersonCollectionChangedEventPublisher(eventPublisher, businessUnit));
			_messageSenders.Add(new PersonPeriodChangedBusMessagePublisher(sender));
			if (_toggleManager.IsEnabled(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
			{
				_messageSenders.Add(new ScheduleChangedMessageSender(_messageSender, CurrentDataSource.Make(), businessUnit,
					_serializer, _initiatorIdentifier));
			}
		}

		public IEnumerable<IPersistCallback> Current()
		{
			return _threadMessageSenders ?? _globalMessageSenders ?? _messageSenders;
		}

		public IDisposable GloballyUse(IEnumerable<IPersistCallback> messageSenders)
		{
			_globalMessageSenders = messageSenders;
			return new GenericDisposable(() =>
			{
				_globalMessageSenders = null;
			});
		}

		public IDisposable OnThisThreadUse(IEnumerable<IPersistCallback> messageSenders)
		{
			_threadMessageSenders = messageSenders;
			return new GenericDisposable(() =>
			{
				_threadMessageSenders = null;
			});
		}

		public IDisposable OnThisThreadExclude<T>()
		{
			return OnThisThreadUse(Current().Where(x => x.GetType() != typeof(T)).ToArray());
		}
	}
}