using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DenormalizationQueueEventsPublisher : IEventsPublisher
	{
		private readonly ISaveToDenormalizationQueue _saveToDenormalizationQueue;

		public DenormalizationQueueEventsPublisher(ISaveToDenormalizationQueue saveToDenormalizationQueue) {
			_saveToDenormalizationQueue = saveToDenormalizationQueue;
		}

		public void Publish(IEnumerable<IEvent> events)
		{
			_saveToDenormalizationQueue.Execute(new EventsMessage() {Events = events});
		}
	}
}