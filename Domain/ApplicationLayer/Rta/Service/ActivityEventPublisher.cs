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
			if (info.Schedule.CurrentActivityId() == info.PreviousActivityId || info.CurrentActivity == null) return;

			var previousStateTime = info.Previous.ReceivedTime;
			var activityStartedInThePast = info.CurrentActivity.StartDateTime < previousStateTime;
			var startTime = activityStartedInThePast
				? previousStateTime
				: info.CurrentActivity.StartDateTime;
			var adherenceChanged = info.Adherence.AdherenceForPreviousState() != info.Adherence.AdherenceForPreviousStateAndCurrentActivity();

			_eventPublisher.Publish(info, new PersonActivityStartEvent
			{
				PersonId = info.Person.PersonId,
				StartTime = startTime,
				Name = info.CurrentActivity.Name,
				Adherence = info.Adherence.AdherenceForPreviousStateAndCurrentActivityForEvent(),
			});

			if (adherenceChanged)
				_adherenceEventPublisher.Publish(info, startTime, info.Adherence.AdherenceForPreviousStateAndCurrentActivity());
		}
	}
}