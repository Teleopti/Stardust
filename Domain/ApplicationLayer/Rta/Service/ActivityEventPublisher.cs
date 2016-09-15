using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly AdherenceEventPublisher _adherenceEventPublisher;

		public ActivityEventPublisher(IEventPopulatingPublisher eventPublisher, AdherenceEventPublisher adherenceEventPublisher)
		{
			_eventPublisher = eventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
		}

		public void Publish(Context info)
		{
			var currentActivity = info.Schedule.CurrentActivity();

			if (info.Schedule.CurrentActivityId() == info.Stored?.ActivityId)
				return;
			
			DateTime? activityStartTime = null; 
			if (currentActivity != null)
			{
				var previousStateTime = info.Stored.ReceivedTime();
				var activityStartedInThePast = currentActivity.StartDateTime < previousStateTime;
				activityStartTime = activityStartedInThePast
					? previousStateTime
					: currentActivity.StartDateTime;

				_eventPublisher.Publish(new PersonActivityStartEvent
				{
					BelongsToDate = info.Schedule.BelongsToDate,
					PersonId = info.PersonId,
					StartTime = activityStartTime.Value,
					Name = currentActivity.Name,
					Adherence = info.Adherence.AdherenceForStoredStateAndCurrentActivity(),
				});
			}

			if (info.Adherence.AdherenceChangedFromActivityChange())
			{
				DateTime timeOfAdherenceChange ;
				if (activityStartTime != null)
					timeOfAdherenceChange = activityStartTime.Value;
				else if (info.Schedule.PreviousActivity() != null)
					timeOfAdherenceChange = info.Schedule.PreviousActivity().EndDateTime;
				else
					timeOfAdherenceChange = info.CurrentTime;
					
				_adherenceEventPublisher.Publish(info, timeOfAdherenceChange, info.Adherence.AdherenceForStoredStateAndCurrentActivity());
			}
		}
	}
}