using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

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
			if (info.CurrentActivityId == info.PreviousActivityId || info.CurrentActivity == null) return;

			var previousStateTime = info.PreviousStateTime;
			var activityStartedInThePast = info.CurrentActivity.StartDateTime < previousStateTime;
			var startTime = activityStartedInThePast
				? previousStateTime
				: info.CurrentActivity.StartDateTime;
			var adherenceChanged = info.AdherenceForPreviousState != info.AdherenceForPreviousStateAndCurrentActivity;

			_eventPublisher.Publish(new PersonActivityStartEvent
			{
				PersonId = info.PersonId,
				StartTime = startTime,
				Name = info.CurrentActivity.Name,
				BusinessUnitId = info.BusinessUnitId,
				InAdherence = info.AdherenceForPreviousStateAndCurrentActivity == Adherence.In,
				ScheduleDate = info.IsScheduled ? new DateOnly(info.CurrentShiftStartTime) : new DateOnly(startTime),
				ShiftEndTime = info.IsScheduled ? info.CurrentShiftEndTime : startTime
			});

			if (adherenceChanged)
				_adherenceEventPublisher.Publish(info, startTime, info.AdherenceForPreviousStateAndCurrentActivity);
		}
	}
}