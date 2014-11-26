using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ActivityEventPublisher : IActivityEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IAdherenceEventPublisher _adherenceEventPublisher;
		private readonly INow _now;

		public ActivityEventPublisher(
			IEventPopulatingPublisher eventPublisher, 
			IAdherenceEventPublisher adherenceEventPublisher,
			INow now)
		{
			_eventPublisher = eventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
			_now = now;
		}

		public void Publish(StateInfo info)
		{
			if (info.NewState.ScheduledId == info.PreviousState.ScheduledId || info.CurrentActivity == null) return;

			var activityStartedInThePast = info.CurrentActivity.StartDateTime < info.PreviousState.ReceivedTime;
			var startTime = activityStartedInThePast
				? _now.UtcDateTime()
				: info.CurrentActivity.StartDateTime;

			_eventPublisher.Publish(new PersonActivityStartEvent
			{
				PersonId = info.NewState.PersonId,
				StartTime = startTime,
				Name = info.CurrentActivity.Name,
				BusinessUnitId = info.NewState.BusinessUnitId,
				InAdherence = info.AdherenceForNewActivity
			});

			_adherenceEventPublisher.Publish(info, startTime, info.AdherenceForNewActivity);
		}
	}
}