using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Historical.AdjustAdherence
{
	public class AdjustAdherenceToNeutral
	{
		private readonly IEventPublisher _publisher;
		private readonly IRtaEventStoreAsyncSynchronizer _synchronizer;

		public AdjustAdherenceToNeutral(IEventPublisher publisher, IRtaEventStoreAsyncSynchronizer synchronizer)
		{
			_publisher = publisher;
			_synchronizer = synchronizer;
		}

		public void Adjust(AdjustedPeriod period)
		{
			var validPeriod = new DateTimePeriod(period.StartTime, period.EndTime);
			
			_publisher.Publish(new PeriodAdjustedToNeutralEvent
			{
				StartTime = validPeriod.StartDateTime,
				EndTime = validPeriod.EndDateTime
			});
			
			_synchronizer.SynchronizeAsync();			
		}
	}
}