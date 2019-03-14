using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Historical.Approval
{
	public class Approval
	{
		private readonly IEventPublisher _publisher;
		private readonly BelongsToDateMapper _belongsToDate;
		private readonly IRtaEventStoreAsyncSynchronizer _synchronizer;

		public Approval(
			IEventPublisher publisher,
			BelongsToDateMapper belongsToDate,
			IRtaEventStoreAsyncSynchronizer synchronizer)
		{
			_publisher = publisher;
			_belongsToDate = belongsToDate;
			_synchronizer = synchronizer;
		}

		public void ApproveAsInAdherence(PeriodToApprove periodToApprove)
		{
			var p = new DateTimePeriod(periodToApprove.StartTime, periodToApprove.EndTime);
			_publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = periodToApprove.PersonId,
				BelongsToDate = _belongsToDate.BelongsToDate(periodToApprove.PersonId, p.StartDateTime, p.EndDateTime),
				StartTime = p.StartDateTime,
				EndTime = p.EndDateTime
			});
			_synchronizer.SynchronizeAsync();
		}

		public void Cancel(PeriodToCancel periodToCancel)
		{
			_publisher.Publish(new PeriodApprovalAsInAdherenceCanceledEvent
			{
				PersonId = periodToCancel.PersonId,
				BelongsToDate = _belongsToDate.BelongsToDate(periodToCancel.PersonId, periodToCancel.StartTime, periodToCancel.EndTime),
				StartTime = periodToCancel.StartTime,
				EndTime = periodToCancel.EndTime
			});
			_synchronizer.SynchronizeAsync();
		}
	}
}