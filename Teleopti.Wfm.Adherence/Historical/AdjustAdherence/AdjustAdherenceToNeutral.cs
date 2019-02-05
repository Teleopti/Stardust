using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Historical.AdjustAdherence
{
	public class AdjustAdherenceToNeutral
	{
		private readonly IEventPublisher _publisher;

		public AdjustAdherenceToNeutral(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Adjust(AdjustedPeriod period)
		{
			var validPeriod = new DateTimePeriod(period.StartTime, period.EndTime);
			
			_publisher.Publish(new PeriodAdjustedToNeutralEvent
			{
				StartTime = validPeriod.StartDateTime,
				EndTime = validPeriod.EndDateTime
			});
		}
	}
}