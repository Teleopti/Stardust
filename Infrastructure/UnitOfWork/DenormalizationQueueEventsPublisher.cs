﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DenormalizationQueueEventsPublisher : IEventsPublisher
	{
		private readonly ISaveToDenormalizationQueue _saver;
		private readonly ISendDenormalizeNotification _notifier;

		public DenormalizationQueueEventsPublisher(ISaveToDenormalizationQueue saver, ISendDenormalizeNotification notifier)
		{
			_saver = saver;
			_notifier = notifier;
		}

		public void Publish(IEnumerable<IEvent> events)
		{
			_saver.Execute(new EventsMessage() {Events = events});
			_notifier.Notify();
		}
	}
}