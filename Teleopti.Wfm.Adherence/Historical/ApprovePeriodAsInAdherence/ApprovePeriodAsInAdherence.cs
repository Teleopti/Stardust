using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherence
	{
		private readonly IEventPublisher _publisher;
		private readonly BelongsToDateMapper _belongsToDate;
		private readonly IRtaEventStoreAsyncSynchronizer _synchronizer;

		public ApprovePeriodAsInAdherence(
			IEventPublisher publisher,
			BelongsToDateMapper belongsToDate,
			IRtaEventStoreAsyncSynchronizer synchronizer)
		{
			_publisher = publisher;
			_belongsToDate = belongsToDate;
			_synchronizer = synchronizer;
		}

		public void Approve(ApprovedPeriod period)
		{
			var p = new DateTimePeriod(period.StartTime, period.EndTime);
			_publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = period.PersonId,
				BelongsToDate = _belongsToDate.BelongsToDate(period.PersonId, p.StartDateTime, p.EndDateTime),
				StartTime = p.StartDateTime,
				EndTime = p.EndDateTime
			});
			_synchronizer.SynchronizeAsync();
		}
	}
}