using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling
{
	public class MailboxSubscriber : IScheduleMessageSubscriber
	{
		private readonly IJsonDeserializer _deserializer;

		public MailboxSubscriber(IJsonDeserializer deserializer)
		{
			_deserializer = deserializer;
		}

		public void Subscribe(Guid scenario, DateTimePeriod period, EventHandler<EventMessageArgs> onEventMessage)
		{
			var aggregatedScheduleChangeEventHandler = new EventHandler<EventMessageArgs>((sender, args) =>
			{
				var message = args.InternalMessage;
				foreach (var personId in _deserializer.DeserializeObject<Guid[]>(message.BinaryData))
				{
					onEventMessage(sender, new EventMessageArgs(new EventMessage
					{
						InterfaceType = typeof(IScheduleChangedEvent),
						DomainObjectType = typeof(IScheduleChangedEvent).Name,
						DomainObjectId = personId,
						ModuleId = message.ModuleIdAsGuid(),
						ReferenceObjectId = message.DomainReferenceIdAsGuid(),
						EventStartDate = message.StartDateAsDateTime(),
						EventEndDate = message.EndDateAsDateTime(),
						DomainUpdateType = message.DomainUpdateTypeAsDomainUpdateType(),
					}));
				}
			});

			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				aggregatedScheduleChangeEventHandler,
				scenario,
				typeof(Scenario),
				typeof(IAggregatedScheduleChange),
				period.StartDateTime,
				period.EndDateTime,
				false,
				true);

			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersistableScheduleData),
				period.StartDateTime,
				period.EndDateTime,
				true,
				true);

			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				typeof(IMeeting),
				true,
				true);

			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(
				onEventMessage,
				typeof(IPersonRequest),
				true,
				true);
		}
	}
}