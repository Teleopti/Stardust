using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CompositeScheduleEventsPublisher : ITransactionHook
	{
		private readonly ScheduleChangedEventPublisher _scheduleChangedEventPublisher;
		private readonly EventsMessageSender _eventsMessageSender;

		public CompositeScheduleEventsPublisher(ScheduleChangedEventPublisher scheduleChangedEventPublisher, EventsMessageSender eventsMessageSender)
		{
			_scheduleChangedEventPublisher = scheduleChangedEventPublisher;
			_eventsMessageSender = eventsMessageSender;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			_scheduleChangedEventPublisher.AfterCompletion(modifiedRoots);
			_eventsMessageSender.AfterCompletion(modifiedRoots);
		}
	}
}