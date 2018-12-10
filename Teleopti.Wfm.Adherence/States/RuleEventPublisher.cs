using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public class RuleEventPublisher
	{
		private readonly IEventPopulatingPublisher _publisher;

		public RuleEventPublisher(IEventPopulatingPublisher publisher)
		{
			_publisher = publisher;
		}

		public void Publish(Context context)
		{
			if (!context.State.RuleChanged()) return;

			_publisher.Publish(new PersonRuleChangedEvent
			{
				PersonId = context.PersonId,
				Timestamp = context.Time,
				BelongsToDate = context.Schedule.BelongsToDate,

				StateName = context.State.StateGroupName(),
				StateGroupId = context.State.StateGroupId(),

				ActivityName = context.Schedule.CurrentActivityName(),
				ActivityColor = context.Schedule.CurrentActivity()?.DisplayColor,

				RuleName = context.State.RuleName(),
				RuleColor = context.State.RuleDisplayColor(),

				Adherence = context.Adherence.Adherence(),
				StaffingEffect = context.State.StaffingEffect(),
				IsAlarm = context.IsAlarm,
				AlarmStartTime = context.AlarmStartTime,
				AlarmColor = context.State.AlarmColor(),
			});
		}
	}
}