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

			var activityStartedInThePast = info.CurrentActivity.StartDateTime < info.PreviousState.ReceivedTime;
			var startTime = activityStartedInThePast
				? info.PreviousState.ReceivedTime
				: info.CurrentActivity.StartDateTime;

			_eventPublisher.Publish(new PersonActivityStartEvent
			{
				PersonId = info.NewState.PersonId,
				StartTime = startTime,
				Name = info.CurrentActivity.Name,
				BusinessUnitId = info.NewState.BusinessUnitId,
				InAdherence = info.AdherenceForPreviousStateAndCurrentActivity == Adherence.In
			});

			_adherenceEventPublisher.Publish(info, startTime, info.AdherenceForPreviousState, info.AdherenceForPreviousStateAndCurrentActivity);
		}
	}
}