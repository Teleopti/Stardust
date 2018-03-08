using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	public class RemoveApprovedPeriod
	{
		private readonly IEventPublisher _publisher;

		public RemoveApprovedPeriod(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Remove(RemovedPeriod period)
		{		
			_publisher.Publish(new ApprovedPeriodRemovedEvent 
			{
				PersonId = period.PersonId,
				StartTime = period.StartTime,
				EndTime = period.EndTime
			});
		}
	}
}