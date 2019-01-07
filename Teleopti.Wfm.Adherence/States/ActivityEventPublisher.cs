using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.States
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
					StartTime = activityStartTime(context),
					Name = currentActivity.Name
				});
			}

		}

		private DateTime activityStartTime(Context context)
		{
			var currentActivity = context.Schedule.CurrentActivity();
			var previousStateTime = context.Stored.ReceivedTime();
			var activityStartedInThePast = currentActivity.StartDateTime < previousStateTime;
			return activityStartedInThePast
				? context.Time
				: currentActivity.StartDateTime;
		}

	}
}