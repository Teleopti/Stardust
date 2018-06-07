using System;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Interfaces.Domain;

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
		private const int threshold = 59;

		public LateForWorkEventPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Publish(Context context)
		{
			var timeSinceShiftStart = context.Time - context.Schedule.CurrentShiftStartTime;

			if (context.Stored.LateForWork && context.State.IsLoggedIn() &&  timeSinceShiftStart.TotalSeconds > threshold)
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
	}
}