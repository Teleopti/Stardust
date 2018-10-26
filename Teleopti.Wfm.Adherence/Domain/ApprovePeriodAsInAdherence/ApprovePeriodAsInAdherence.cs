using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence
{
	public class ApprovePeriodAsInAdherence
	{
		private readonly IEventPublisher _publisher;
		private readonly BelongsToDateMapper _belongsToDate;

		public ApprovePeriodAsInAdherence(IEventPublisher publisher, BelongsToDateMapper belongsToDate)
		{
			_publisher = publisher;
			_belongsToDate = belongsToDate;
		}

		public void Approve(ApprovedPeriod period)
		{
			var p = new DateTimePeriod(period.StartTime, period.EndTime);
			_publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = period.PersonId,
				StartTime = p.StartDateTime,
				EndTime = p.EndDateTime,
				BelongsToDate = _belongsToDate.BelongsToDate(period.PersonId, p.StartDateTime, p.EndDateTime)
			});
		}
	}
}