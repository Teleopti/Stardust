using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public class AdherenceEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public AdherenceEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context context)
		{
			var toAdherence = context.Adherence.Adherence();

			if (context.Stored.Adherence == toAdherence)
				return;

			var time = context.Time;
			
			if (toAdherence == EventAdherence.In)
			{
				_eventPublisher.Publish(new PersonInAdherenceEvent
				{
					BelongsToDate = context.Schedule.BelongsToDate,
					PersonId = context.PersonId,
					Timestamp = time,
					BusinessUnitId = context.BusinessUnitId,
					TeamId = context.TeamId,
					SiteId = context.SiteId
				});
			}

			if (toAdherence == EventAdherence.Out)
			{
				_eventPublisher.Publish(new PersonOutOfAdherenceEvent
				{
					BelongsToDate = context.Schedule.BelongsToDate,
					PersonId = context.PersonId,
					Timestamp = time,
					BusinessUnitId = context.BusinessUnitId,
					TeamId = context.TeamId,
					SiteId = context.SiteId
				});
			}

			if (toAdherence == EventAdherence.Neutral)
			{
				_eventPublisher.Publish(new PersonNeutralAdherenceEvent
				{
					BelongsToDate = context.Schedule.BelongsToDate,
					PersonId = context.PersonId,
					Timestamp = time,
					BusinessUnitId = context.BusinessUnitId,
					TeamId = context.TeamId,
					SiteId = context.SiteId
				});
			}
		}
	}
}