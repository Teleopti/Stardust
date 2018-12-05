using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public class LateForWorkEventPublisher
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly TimeSpan threshold = TimeSpan.FromMinutes(1);

		public LateForWorkEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context context)
		{
			var arrivingAfterLateForWork = context.Schedule.OngoingShift() &&
										   context.State.IsLoggedIn() &&
										   previousStateChangeWasBeforeShift(context) &&
										   previousStateChangeWasLoggedOut(context) &&
										   isAfterThreshold(context);

			if (arrivingAfterLateForWork)
				_eventPublisher.Publish(new PersonArrivedLateForWorkEvent
				{
					PersonId = context.PersonId,
					Timestamp = context.Time,
					BelongsToDate = context.Schedule.BelongsToDate,
					ActivityName = context.Schedule.CurrentActivityName(),
					ActivityColor = context.Schedule.CurrentActivity()?.DisplayColor,
					StateName = context.State.StateGroupName(),
					ShiftStart = context.Schedule.CurrentShiftStartTime,
					RuleName = context.State.RuleName(),
					RuleColor = context.State.RuleDisplayColor(),
					Adherence = context.Adherence.Adherence()
				});
		}

		private static bool previousStateChangeWasLoggedOut(Context context) => context.State.IsLoggedOut(context.Stored.StateGroupId);
		private static bool previousStateChangeWasBeforeShift(Context context) => context.Stored.StateStartTime == null || context.Stored.StateStartTime < context.Schedule.CurrentShiftStartTime;
		private bool isAfterThreshold(Context context) => context.Time >= context.Schedule.CurrentShiftStartTime + threshold;
	}
}