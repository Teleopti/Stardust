using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface ILateForWorkEventPublisher
	{
		void Publish(Context context);
	}

	public class NoLateForWorkEventPublisher : ILateForWorkEventPublisher
	{
		public void Publish(Context context)
		{
		}
	}

	public class LateForWorkEventPublisher : ILateForWorkEventPublisher
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
				_eventPublisher.Publish(new PersonArrivalAfterLateForWorkEvent
				{
					PersonId = context.PersonId,
					ActivityName = context.Schedule.CurrentActivityName(),
					ActivityColor = context.Schedule.CurrentActivity()?.DisplayColor,
					StateName = context.State.StateGroupName(),
					ShiftStart = context.Schedule.CurrentShiftStartTime,
					Timestamp = context.Time,
					RuleName = context.State.RuleName(),
					RuleColor = context.State.RuleDisplayColor(),
					Adherence = context.Adherence.Adherence()
				});
		}

		private static bool previousStateChangeWasLoggedOut(Context context) => context.StateMapper.LoggedOutStateGroupIds().Contains(context.Stored.StateGroupId.Value);
		private static bool previousStateChangeWasBeforeShift(Context context) => context.Stored.StateStartTime == null || context.Stored.StateStartTime < context.Schedule.CurrentShiftStartTime;
		private bool isAfterThreshold(Context context) => context.Time >= context.Schedule.CurrentShiftStartTime + threshold;
	}
}