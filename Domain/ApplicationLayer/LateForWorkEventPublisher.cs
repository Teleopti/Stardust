using System;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer
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
			var arrivingAfterLateForWork = context.Stored.LateForWork &&
										   context.State.IsLoggedIn() &&
										   isOutsideTreshold(context.Time, context.Schedule.CurrentShiftStartTime);

			var lateForWork = context.Schedule.ShiftStarted() && context.State.IsLoggedOut();
			if (!arrivingAfterLateForWork)
				context.LateForWork = context.Stored.LateForWork || lateForWork;

			if (context.Schedule.ShiftEnded())
			{
				context.LateForWork = false;
				arrivingAfterLateForWork = false;
			}

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

		private bool isOutsideTreshold(DateTime stateTime, DateTime shiftStart)
		{
			var timeSinceShiftStart = stateTime - shiftStart;
			return timeSinceShiftStart.TotalSeconds > threshold.TotalSeconds;
		}
	}
}