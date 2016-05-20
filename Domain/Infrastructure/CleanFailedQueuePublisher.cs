using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class CleanFailedQueuePublisher
	{
		private readonly IRecurringEventPublisher _recurringEventPublisher;

		public CleanFailedQueuePublisher(IRecurringEventPublisher recurringEventPublisher)
		{
			_recurringEventPublisher = recurringEventPublisher;
		}

		public void Start()
		{
			_recurringEventPublisher.PublishHourly(new CleanFailedQueue());
		}
	}
}