using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ActivityEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context info)
		{
			if (!info.Schedule.ActivityChanged())
				return;

			var currentActivity = info.Schedule.CurrentActivity();

			if (currentActivity != null)
			{
				_eventPublisher.Publish(new PersonActivityStartEvent
				{
					BelongsToDate = info.Schedule.BelongsToDate,
					PersonId = info.PersonId,
					StartTime = info.Schedule.ActivityStartTime().Value,
					Name = currentActivity.Name,
					Adherence = info.Adherence.AdherenceForStoredStateAndCurrentActivity(),
				});
			}

		}
	}
}