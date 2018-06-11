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
		private readonly TimeSpan threshold = TimeSpan.FromSeconds(59);

		public LateForWorkEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context context)
		{
			if (context.Schedule.CurrentActivity() == null)
				return;

			var timeSinceShiftStart = context.Time - context.Schedule.CurrentShiftStartTime;
			var isAfterThreshold = timeSinceShiftStart.TotalSeconds > threshold.TotalSeconds;

			var arrivingAfterLateForWork = previousStateWasBeforeShiftAndLoggedOff(context) &&
										   context.State.IsLoggedIn() &&
										   isAfterThreshold;

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

		private static bool previousStateWasBeforeShiftAndLoggedOff(Context context)
		{
			if (context.Stored.StateGroupId == null)
				return true;
			if (context.Stored.StateStartTime == null)
				return true;
			if (context.Stored.StateStartTime > context.Schedule.CurrentShiftStartTime)
				return false;
			if (context.StateMapper.LoggedOutStateGroupIds().Contains(context.Stored.StateGroupId.Value))
				return true;
			return false;
		}

	}
}