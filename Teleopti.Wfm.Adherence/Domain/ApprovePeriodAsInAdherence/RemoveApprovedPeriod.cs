using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence
{
	public class RemoveApprovedPeriod
	{
		private readonly IEventPublisher _publisher;
		private readonly BelongsToDateMapper _belongsToDate;

		public RemoveApprovedPeriod(IEventPublisher publisher, BelongsToDateMapper belongsToDate)
		{
			_publisher = publisher;
			_belongsToDate = belongsToDate;
		}

		public void Remove(RemovedPeriod period)
		{		
			_publisher.Publish(new ApprovedPeriodRemovedEvent 
			{
				PersonId = period.PersonId,
				BelongsToDate = _belongsToDate.BelongsToDate(period.PersonId, period.StartTime, period.EndTime),
				StartTime = period.StartTime,
				EndTime = period.EndTime
			});
		}
	}
}