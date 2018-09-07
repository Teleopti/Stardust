using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherence
	{
		private readonly IEventPublisher _publisher;

		public ApprovePeriodAsInAdherence(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Approve(ApprovedPeriod period)
		{
			var p = new DateTimePeriod(period.StartTime, period.EndTime);
			_publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = period.PersonId,
				StartTime = p.StartDateTime,
				EndTime = p.EndDateTime
			});
		}
	}
}