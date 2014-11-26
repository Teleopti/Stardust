using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ActivityEventPublisher : IActivityEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IAdherenceEventPublisher _adherenceEventPublisher;

		public ActivityEventPublisher(IEventPopulatingPublisher eventPublisher, IAdherenceEventPublisher adherenceEventPublisher)
		{
			_eventPublisher = eventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
		}

		public void Publish(StateInfo info)
		{
			if (info.NewState.ScheduledId == info.PreviousState.ScheduledId || info.CurrentActivity == null) return;

			_eventPublisher.Publish(new PersonActivityStartEvent
			{
				PersonId = info.NewState.PersonId,
				StartTime = info.CurrentActivity.StartDateTime,
				Name = info.CurrentActivity.Name,
				BusinessUnitId = info.NewState.BusinessUnitId,
				InAdherence = info.AdherenceForNewActivity
			});

			_adherenceEventPublisher.Publish(info, info.CurrentActivity.StartDateTime, info.AdherenceForNewActivity);
		}
	}
}