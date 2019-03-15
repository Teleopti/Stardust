using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Historical.Adjustment
{
	public class Adjustment
	{
		private readonly IEventPublisher _publisher;
		private readonly IRtaEventStoreAsyncSynchronizer _synchronizer;

		public Adjustment(IEventPublisher publisher, IRtaEventStoreAsyncSynchronizer synchronizer)
		{
			_publisher = publisher;
			_synchronizer = synchronizer;
		}

		public void AdjustToNeutral(PeriodToAdjust periodToAdjust)
		{
			var validPeriod = new DateTimePeriod(periodToAdjust.StartTime, periodToAdjust.EndTime);

			_publisher.Publish(new PeriodAdjustedToNeutralEvent
			{
				StartTime = validPeriod.StartDateTime,
				EndTime = validPeriod.EndDateTime
			});

			_synchronizer.SynchronizeAsync();
		}

		public void Cancel(PeriodToCancel periodToCancel)
		{
			new DateTimePeriod(periodToCancel.StartTime, periodToCancel.EndTime);

			_publisher.Publish(new PeriodAdjustmentToNeutralCanceledEvent
			{
				StartTime = periodToCancel.StartTime,
				EndTime = periodToCancel.EndTime
			});
			
			_synchronizer.SynchronizeAsync();
		}
	}
}