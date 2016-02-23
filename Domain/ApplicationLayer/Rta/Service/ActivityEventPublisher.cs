using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityEventPublisher : IActivityEventPublisher
	{
		private readonly IRtaDecoratingEventPublisher _eventPublisher;
		private readonly IAdherenceEventPublisher _adherenceEventPublisher;

		public ActivityEventPublisher(IRtaDecoratingEventPublisher eventPublisher, IAdherenceEventPublisher adherenceEventPublisher)
		{
			_eventPublisher = eventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
		}

		public void Publish(StateInfo info)
		{
			var currentActivity = info.Schedule.CurrentActivity();

			if (info.Schedule.CurrentActivityId() == info.Stored.ActivityId())
				return;
			
			DateTime? activityStartTime = null; 
			if (currentActivity != null)
			{
				var previousStateTime = info.Stored.ReceivedTime();
				var activityStartedInThePast = currentActivity.StartDateTime < previousStateTime;
				activityStartTime = activityStartedInThePast
					? previousStateTime
					: currentActivity.StartDateTime;

				_eventPublisher.Publish(info, new PersonActivityStartEvent
				{
					PersonId = info.Person.PersonId,
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