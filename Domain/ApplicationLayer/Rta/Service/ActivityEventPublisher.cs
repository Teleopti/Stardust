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
			if (info.Schedule.CurrentActivityId() == info.PreviousActivityId || currentActivity == null) return;

			var previousStateTime = info.Previous.ReceivedTime;
			var activityStartedInThePast = currentActivity.StartDateTime < previousStateTime;
			var startTime = activityStartedInThePast
				? previousStateTime
				: currentActivity.StartDateTime;
			var adherenceChanged = info.Adherence.AdherenceForPreviousState() != info.Adherence.AdherenceForPreviousStateAndCurrentActivity();

			_eventPublisher.Publish(info, new PersonActivityStartEvent
			{
				PersonId = info.Person.PersonId,
				StartTime = startTime,
				Name = currentActivity.Name,
				Adherence = info.Adherence.AdherenceForPreviousStateAndCurrentActivityForEvent(),
			});

			if (adherenceChanged)
				_adherenceEventPublisher.Publish(info, startTime, info.Adherence.AdherenceForPreviousStateAndCurrentActivity());
		}
	}
}