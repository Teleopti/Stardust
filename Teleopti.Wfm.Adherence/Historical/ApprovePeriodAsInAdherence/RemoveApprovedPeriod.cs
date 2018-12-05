using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence
{
	public class RemoveApprovedPeriod
	{
		private readonly IEventPublisher _publisher;
		private readonly BelongsToDateMapper _belongsToDate;
		private readonly IRtaEventStoreAsyncSynchronizer _synchronizer;

		public RemoveApprovedPeriod(
			IEventPublisher publisher,
			BelongsToDateMapper belongsToDate,
			IRtaEventStoreAsyncSynchronizer synchronizer
		)
		{
			_publisher = publisher;
			_belongsToDate = belongsToDate;
			_synchronizer = synchronizer;
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
			_synchronizer.SynchronizeAsync();
		}
	}
}