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

		public void Publish(Context context)
		{
			if (!context.Schedule.ActivityChanged())
				return;

			var currentActivity = context.Schedule.CurrentActivity();

			if (currentActivity != null)
			{
				_eventPublisher.Publish(new PersonActivityStartEvent
				{
					BelongsToDate = context.Schedule.BelongsToDate,
					PersonId = context.PersonId,
					StartTime = context.Schedule.ActivityStartTime().Value,
					Name = currentActivity.Name,
					Adherence = context.Adherence.AdherenceForStoredStateAndCurrentActivity(),
				});
			}

		}
	}
}