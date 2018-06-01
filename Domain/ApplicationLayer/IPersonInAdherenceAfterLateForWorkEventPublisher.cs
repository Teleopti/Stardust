﻿using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
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

		public LateForWorkEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context context)
		{
			var isInAlarm = context.Time >= context.AlarmStartTime;
			//var wasInAlarm = context.Time >= context.Stored.AlarmStartTime;
			if (context.Time > context.Stored.AlarmStartTime && !isInAlarm)
			{
				_eventPublisher.Publish(new PersonArrivalAfterLateForWorkEvent()
				{
					PersonId = context.PersonId,
					ActivityName = context.Schedule.CurrentActivityName(),
					ActivityColor = context.Schedule.CurrentActivity()?.DisplayColor,
					StateName = context.State.StateGroupName(),
					ShiftStart = context.Schedule.CurrentShiftStartTime,
					Timestamp = context.Time,
					RuleName = context.State.RuleName(),
					RuleColor = context.State.RuleDisplayColor(),
					Adherence = context.Adherence.Adherence(),
				});
			}
		}
	}
}